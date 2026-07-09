using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.ModelManagement.Interface;
using AI.Interviewer.Services.Models;
using AI.Interviewer.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whisper.net;
using Whisper.net.LibraryLoader;

namespace AI.Interviewer.Services.Concrete;

public class WhisperSTTService : IWhisperSTTService, IDisposable
{
    private readonly IModelManager _modelManager;
    private readonly WhisperOptions _options;
    private readonly ILogger<WhisperSTTService>? _logger;
    private WhisperProcessor? _processor;
    private string _executionMode = "Unknown";
    private bool _disposed;

    public bool IsInitialized => _processor != null;

    public WhisperSTTService(IModelManager modelManager, IOptions<WhisperOptions> options, ILogger<WhisperSTTService>? logger = null)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
        {
            _logger?.LogDebug("WhisperSTTService already initialized, skipping");
            return Task.CompletedTask;
        }

        // add into background thread
        return Task.Run(() =>
        {
            try
            {
                string modelPath = _modelManager.GetWhisperModelPath();
                _logger?.LogInformation("Initialization Whisper STT with model at: {modelPath}", modelPath);

                var factory = WhisperFactory.FromPath(modelPath);

                RuntimeOptions.RuntimeLibraryOrder = new List<RuntimeLibrary>
                {
                    RuntimeLibrary.Cuda,
                    RuntimeLibrary.Cuda12,
                    RuntimeLibrary.CoreML,
                    RuntimeLibrary.OpenVino,
                    RuntimeLibrary.Cpu
                };

                var builder = factory.CreateBuilder().WithLanguage(_options.Language).WithThreads(_options.Threads);
                _processor = builder.Build();
                _executionMode = DetectExecutionModeForHeuristic();

                _logger?.LogInformation("Whisper STT initialized successfully");
                _logger?.LogInformation("Execution Mode: {ExecutionMode} | Language: {Language} | Threads: {Threads}", _executionMode, _options.Language, _options.Threads);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize Whisper processor");
                throw;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Best-effort diagnostic only — inspects loaded process modules for CUDA-related
    /// libraries. This is NOT a reliable confirmation of which runtime Whisper.net
    /// actually selected at inference time; treat it as a hint for logs, not a fact
    /// to branch logic on.
    /// </summary>
    private string DetectExecutionModeForHeuristic()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            bool cudaModuleLoaded = process.Modules.Cast<ProcessModule>().Any(m => m.ModuleName.Contains("cuda", StringComparison.OrdinalIgnoreCase));

            return cudaModuleLoaded ? "GPU (CUDA) - heuristic" : "CPU (assumed) - heuristic";
        } catch(Exception ex) {
            _logger?.LogDebug(ex, "Could not determined execution mode heuristic");
            return "Unknown";
        }
    }

    /// <summary>
    /// Transcribes audio from a stream
    /// Used for real-time streaming from SignalR
    /// </summary>
    public async Task<TranscriptionResult> TranscribeAudioStreamAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        if(_disposed)
        {
            throw new ObjectDisposedException(nameof(WhisperSTTService));
        }

        if(!IsInitialized || _processor == null)
        {
            _logger?.LogError("Whisper Processor not initialized - call InitializeAsync() first");
            return new TranscriptionResult
            {
                Success = false,
                ErrorMessage = "Whisper processor not initialized. Call InitializeAsync() before transcribing."
            };
        }

        if(_processor == null)
        {
            _logger?.LogError("Whisper Processor not initialized");
            return new TranscriptionResult
            {
                Success = false,
                ErrorMessage = "Whisper processor not initialized"
            };
        }

        if(audioStream == null)
        {
            _logger?.LogWarning("Null audio stream provided");
            return new TranscriptionResult
            {
                Success = false,
                ErrorMessage = "Audio stream is null"
            };
        }

        if(audioStream.CanSeek &&  audioStream.Length == 0)
        {
            _logger?.LogWarning("Empty audio stream provided");
            return new TranscriptionResult
            {
                Success = false,
                ErrorMessage = "Audio stream is empty"
            };
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger?.LogDebug("Transcibing audio stream ({ExecutionMode})...", _executionMode);
            var resultBuilder = new StringBuilder();
            
            await foreach(var segment in _processor.ProcessAsync(audioStream, cancellationToken))
            {
                resultBuilder.Append(segment.Text);
            }

            var result = resultBuilder.ToString().Trim();
            stopwatch.Stop();
            _logger?.LogInformation("Transrciption completed ({ExecutionMode}) in {ProcessingTime:F2}s", _executionMode, stopwatch.Elapsed.TotalSeconds);


            bool hasUsableText = !string.IsNullOrWhiteSpace(result);
            if(!hasUsableText)
            {
                _logger?.LogWarning("Trancription produced no usable text (empty or whitespace only)");
            }
            return new TranscriptionResult
            {
                Text = result,
                Confidence = 0.85f,
                ProcessingTime = stopwatch.Elapsed,
                Language = _options.Language,
                Success = hasUsableText
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Error transcribing audio stream");

            return new TranscriptionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Disposes the WhisperSTTService
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _processor?.Dispose();
        _disposed = true;
        _logger?.LogInformation("WhisperSTTService disposed");
    }
}
