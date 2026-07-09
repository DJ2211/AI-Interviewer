using AI.Interviewer.Api.State;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.Models;
using AI.Interviewer.Services.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using NAudio.Midi;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.Interviewer.Api.Hubs;

public class InterviewerHub : Hub
{
    private const int SampleRate = 16000; // should match browser's Audioworks
    private readonly IWhisperSTTService _sttService;
    private readonly ITTSService _ttsService;
    private readonly ILLMInterviewerService _llmService;
    private readonly ILogger<InterviewerHub> _logger;
    private readonly InterviewConnectionManager _connections;

    public InterviewerHub(IWhisperSTTService sttService, ITTSService ttsService, ILLMInterviewerService llmService, ILogger<InterviewerHub> logger, InterviewConnectionManager connections)
    {
        _sttService = sttService;
        _llmService = llmService;
        _logger = logger;
        _ttsService = ttsService;
        _connections = connections;
    }

    public async Task StartInterview(InterviewConfiguration config)
    {
        _logger.LogInformation("Starting interview for connection {ConnectionId}", Context.ConnectionId);

        var session = new InterviewSession { Configuration = config };
        _connections.Add(Context.ConnectionId, new InterviewConnectionState { Session = session });

        await StreamAndSpeakAsync(session);
    }

    private async Task StreamAndSpeakAsync(InterviewSession session)
    {
        var sentenceBuffer = new StringBuilder();
        var fullResponse = new StringBuilder();
        bool shouldEnd = false;

        await foreach (var token in _llmService.StreamNextQuestionAsync(session, Context.ConnectionAborted))
        {
            sentenceBuffer.Append(token);
            fullResponse.Append(token);

            if(fullResponse.ToString().Contains("[END_INTERVIEW]"))
            {
                shouldEnd = true;
            }

            // Only treat as sentence-end if punctuation is followed by whitespace (not ".NET", "e.g.", etc.)
            if (Regex.IsMatch(sentenceBuffer.ToString(), @"[.!?]\s$"))
            {
                var sentence = sentenceBuffer.ToString().Replace("[END_INTERVIEW]", "").Trim();
                sentenceBuffer.Clear();
                if (sentence.Length > 0)
                {
                    var audio = await _ttsService.SynthesizeAsync(sentence);
                    await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
                }
            }
        }

        if (sentenceBuffer.Length > 0)
        {
            var sentence = sentenceBuffer.ToString().Replace("[END_INTERVIEW]", "").Trim();
            if(sentence.Length > 0)
            {
                var audio = await _ttsService.SynthesizeAsync(sentence);
                await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
            }
        }

        var cleanResponse = fullResponse.ToString().Replace("[END_INTERVIEW]", "").Trim();
        session.AddTurn(Speaker.Interviewer, cleanResponse);
        await Clients.Caller.SendAsync("InterviewerTurnComplete", shouldEnd);

        if(shouldEnd)
        {
            await Task.Delay(500); // let final audio reach before disconnecting the websocket
            Context.Abort();
        }
    }

    public async Task EndOfSpeech()
    {
        var state = _connections.Get(Context.ConnectionId);
        if(state == null || state.IsProcessingTurn)
        {
            return;
        }

        state.IsProcessingTurn = true;
        await Clients.Caller.SendAsync("ProcessingStarted");
        
        var pcmBytes = state.AudioBuffer.ToArray();
        state.AudioBuffer.Clear();

        if(pcmBytes.Length == 0)
        {
            _logger.LogWarning("EndOfSpeech called with empty audio buffer for {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("TranscriptionFailed", "No Audio Captured");
            return;
        }

        using var wavStream = ConvertPcmBytesToWavStream(pcmBytes, SampleRate);
        var transcription = await _sttService.TranscribeAudioStreamAsync(wavStream);

        if (!transcription.Success || string.IsNullOrWhiteSpace(transcription.Text) || transcription.Text.Split(' ').Length < 2) 
        {
            state.ConsecutiveEmptyTurns++;
            _logger.LogWarning("Empty/short transcripton ({Count} in a row)", state.ConsecutiveEmptyTurns);
            await Clients.Caller.SendAsync("TranscriptionFailed", transcription.ErrorMessage ?? "Empty transcription");

            if(state.ConsecutiveEmptyTurns >= 3)
            {
                state.Session.AddTurn(Speaker.Candidate, "[no clear response - candidate silent or unclear]");
                await StreamAndSpeakAsync(state.Session);
            }
            return;
        }

        state.ConsecutiveEmptyTurns = 0;
        await Clients.Caller.SendAsync("CandidateTranscript", transcription.Text);
        state.Session.AddTurn(Speaker.Candidate, transcription.Text);
        await StreamAndSpeakAsync(state.Session);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.Remove(Context.ConnectionId);
        _logger.LogInformation("Connection {ConnectionId} disconnected, state cleaned up", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Converts raw 16-bit signed PCM bytes (as captured by the browser) into a WAV-wrapped
    /// stream Whisper can consume, by reusing the same WavEncoder built for TTS output.
    /// </summary>
    public static Stream ConvertPcmBytesToWavStream(byte[] pcmBytes, int sampleRate)
    {
        var floatSamples = new float[pcmBytes.Length / 2];
        for(int i = 0; i < floatSamples.Length; i++)
        {
            short sample = BitConverter.ToInt16(pcmBytes, i * 2);
            floatSamples[i] = sample / (float)short.MaxValue;
        }

        var wavBytes = WavEncoder.EncodeToWav(floatSamples, sampleRate);
        return new MemoryStream(wavBytes);
    }

    public Task SendAudioChunk(byte[] chunk)
    {
        var state = _connections.Get(Context.ConnectionId);
        if (state == null || state.IsProcessingTurn) return Task.CompletedTask;

        state.AudioBuffer.AddRange(chunk);
        return Task.CompletedTask;
    }
}
 