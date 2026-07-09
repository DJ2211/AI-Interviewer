using System.Diagnostics;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.Models;
using AI.Interviewer.Services.Options;
using AI.Interviewer.Services.Utilities;
using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Interviewer.Services.Concrete;

public class KokoroTTSService : ITTSService
{
    private readonly TTSOptions _options;
    private readonly ILogger<KokoroTTSService>? _logger;
    private KokoroTTS? _tts;
    private KokoroVoice? _voice;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    public KokoroTTSService(IOptions<TTSOptions> options, ILogger<KokoroTTSService>? logger = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger?.LogDebug("KokoroTTSService already initialized, skipping");
            return Task.CompletedTask;
        }

        return Task.Run(() =>
        {
            try
            {
                _logger?.LogInformation("Loading Kokoro TTS model...");
                _tts = KokoroTTS.LoadModel();

                _voice = KokoroVoiceManager.GetVoice(_options.VoiceName);

                if (_voice == null)
                {
                    throw new InvalidOperationException(
                        $"Voice '{_options.VoiceName}' not found. Check available voices via KokoroVoiceManager.Voices.");
                }

                _isInitialized = true;
                _logger?.LogInformation("Kokoro TTS initialized successfully with voice '{VoiceName}'", _options.VoiceName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize Kokoro TTS");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<SynthesisResult> SynthesizeAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _tts == null || _voice == null)
        {
            throw new InvalidOperationException("KokoroTTSService not initialized. Call InitializeAsync() first.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return new SynthesisResult { Success = false, ErrorMessage = "Text is empty" };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            int[] tokens = Tokenizer.Tokenize(text);

            if (tokens.Length > 510)
            {
                _logger?.LogWarning(
                    "Text produced {TokenCount} tokens, exceeding the 510-token single-job limit. Output may be truncated. Consider segmenting long text.",
                    tokens.Length);
            }

            // Bridge KokoroSharp's callback-based job API to async/await
            var tcs = new TaskCompletionSource<float[]>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

            _tts.EnqueueJob(KokoroJob.Create(tokens, _voice, _options.Speed, samples =>
            {
                tcs.TrySetResult(samples);
            }));

            float[] rawSamples = await tcs.Task;

            int sampleRate = KokoroPlayback.waveFormat.SampleRate;
            byte[] wavBytes = WavEncoder.EncodeToWav(rawSamples, sampleRate);

            stopwatch.Stop();
            _logger?.LogInformation("TTS synthesis completed in {ElapsedSeconds:F2}s", stopwatch.Elapsed.TotalSeconds);

            return new SynthesisResult
            {
                AudioData = wavBytes,
                ProcessingTime = stopwatch.Elapsed,
                Success = true
            };
        }

        catch (OperationCanceledException)
        {
            throw;
        }

        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "TTS synthesis failed");
            return new SynthesisResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }
}