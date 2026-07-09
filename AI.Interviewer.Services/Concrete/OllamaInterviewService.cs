using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.Models;
using AI.Interviewer.Services.Options;
using AI.Interviewer.Services.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AI.Interviewer.Services.Concrete
{
    public class OllamaInterviewService : ILLMInterviewerService
    {
        private readonly LLMOptions _options;
        private readonly ILogger<OllamaInterviewService>? _logger;
        private readonly OllamaApiClient _client;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;
        public OllamaInterviewService(IOptions<LLMOptions> options, ILogger<OllamaInterviewService>? logger = null)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;

            _client = new OllamaApiClient(new Uri(_options.BaseUrl))
            {
                SelectedModel = _options.ModelName
            };
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if(_isInitialized)
            {
                _logger?.LogDebug("OllamaInterviewService already initialized, Skipping...");
                return;
            }
            try
            {
                _logger?.LogInformation("Checking Ollama availabillity at {BaseUrl}...", _options.BaseUrl);

                var localModels = await _client.ListLocalModelsAsync(cancellationToken);

                bool modelAvailable = localModels.Any(m => m.Name.Equals(_options.ModelName, StringComparison.OrdinalIgnoreCase) ||
                m.Name.StartsWith(_options.ModelName.Split(':')[0], StringComparison.OrdinalIgnoreCase));

                if(!modelAvailable)
                {
                    throw new InvalidOperationException(
                        $"Model '{_options.ModelName}' not found in Ollama. Pull it first with: ollama pull {_options.ModelName}"
                        );
                }

                _isInitialized = true;
                _logger?.LogInformation("Ollama initialized successfully with model '{ModelName}'", _options.ModelName);
                
            }catch(Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialized OllamaService");
                throw;
            }
        }

        public async IAsyncEnumerable<string> StreamNextQuestionAsync(InterviewSession session, CancellationToken cancellationToken = default)
        {
            if(!_isInitialized)
            {
                throw new InvalidOperationException("OllamaInterviewService not initialized. Call InitializeAsync() first.");
            }

            if(session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var message = BuildMessage(session);

            var request = new ChatRequest
            {
                Model = _options.ModelName,
                Messages = message,
                Stream = true,
                Options = new RequestOptions
                {
                    Temperature = (float)_options.Temperature,
                    NumPredict = _options.MaxTokens
                }
            };

            await foreach (var chunk in _client.ChatAsync(request, cancellationToken))
            {
                if (chunk?.Message?.Content is { } text)
                {
                    yield return text;
                }
            }
        }

        public async Task<string> GetNextQuestionAsync(InterviewSession session, CancellationToken cancellationToken = default)
        {
            if(!_isInitialized)
            {
                throw new InvalidOperationException("OllamaInterviewService not initialized. Call InitializeAsync() first.");
            }

            if(session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var message = BuildMessage(session);

            var request = new ChatRequest
            {
                Model = _options.ModelName,
                Messages = message,
                Stream = false,
                Options = new RequestOptions
                {
                    Temperature = (float)_options.Temperature,
                    NumPredict = _options.MaxTokens
                }
            };

            var stopwatch = Stopwatch.StartNew();
            var responseBuilder = new StringBuilder();

            try
            {   
                await foreach(var chunk in _client.ChatAsync(request, cancellationToken))
                {
                    if(chunk?.Message?.Content is { } text)
                    {
                        responseBuilder.Append(text);
                    }
                }

                stopwatch.Stop();
                var responseText = responseBuilder.ToString().Trim();

                _logger?.LogInformation("LLM response generated in {ElapsedSeconds:F2}s", stopwatch.Elapsed.TotalSeconds);
                return responseText;
            } 
            catch(Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error generating LLM Response, Please generate Bug in Github if it continues.");
                throw;
            }
        }

        public List<Message> BuildMessage(InterviewSession session)
        {
            var messages = new List<Message>
            {
                new Message(ChatRole.System, InterviewPromptBuilder.Build(session.Configuration))
            };

            foreach(var turn in session.History)
            {
                var role = turn.Speaker == Speaker.Interviewer ? ChatRole.Assistant : ChatRole.User;
                messages.Add(new Message(role, turn.Text));
            }

            return messages;
        }
    }
}
