using AI.Interviewer.Api.State;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.Models;
using AI.Interviewer.Services.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using NAudio.Midi;
using System.Diagnostics.Eventing.Reader;
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

    public InterviewerHub(
        IWhisperSTTService sttService,
        ITTSService ttsService,
        ILLMInterviewerService llmService,
        ILogger<InterviewerHub> logger,
        InterviewConnectionManager connections)
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
        await StreamAndSpeakAsync(session, countAsQuestions: false);
    }

    //private async Task StreamAndSpeakAsync(InterviewSession session, bool countAsQuestions = true)
    //{
    //    var sentenceBuffer = new StringBuilder();
    //    var fullResponse = new StringBuilder();
    //    bool shouldEnd = false;

    //    const int HardFlushLimit = 350; // chars — well under Kokoro's ~510 token ceiling

    //    await foreach (var token in _llmService.StreamNextQuestionAsync(session, Context.ConnectionAborted))
    //    {
    //        sentenceBuffer.Append(token);
    //        fullResponse.Append(token);

    //        if (fullResponse.ToString().Contains("[END_INTERVIEW]"))
    //        {
    //            shouldEnd = true;
    //            _logger.LogWarning("Model emitted [END_INTERVIEW] marker. Full response: {Response}", fullResponse.ToString());
    //        }

    //        // Keep flushing complete sentences out of the buffer as long as any exist,
    //        // regardless of where the current token chunk boundary falls.
    //        while (true)
    //        {
    //            var match = Regex.Match(sentenceBuffer.ToString(), @"^(.*?[.!?])\s+");
    //            if (!match.Success) break;

    //            var sentence = match.Groups[1].Value.Replace("[END_INTERVIEW]", "").Trim();
    //            sentenceBuffer.Remove(0, match.Length);

    //            if (sentence.Length > 0)
    //            {
    //                var audio = await _ttsService.SynthesizeAsync(sentence);
    //                await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
    //            }
    //        }

    //        // Hard safety valve: if no sentence boundary has appeared but the buffer
    //        // is already too long, force a flush at the last word boundary anyway.
    //        if (sentenceBuffer.Length >= HardFlushLimit)
    //        {
    //            var content = sentenceBuffer.ToString();
    //            int cutPoint = content.LastIndexOf(' ');
    //            if (cutPoint <= 0) cutPoint = content.Length;

    //            var forced = content.Substring(0, cutPoint).Replace("[END_INTERVIEW]", "").Trim();
    //            sentenceBuffer.Remove(0, cutPoint);

    //            if (forced.Length > 0)
    //            {
    //                var audio = await _ttsService.SynthesizeAsync(forced);
    //                await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", forced, audio.AudioData);
    //            }
    //        }
    //    }

    //    if (sentenceBuffer.Length > 0)
    //    {
    //        var sentence = sentenceBuffer.ToString().Replace("[END_INTERVIEW]", "").Trim();
    //        if (sentence.Length > 0)
    //        {
    //            var audio = await _ttsService.SynthesizeAsync(sentence);
    //            await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
    //        }
    //    }

    //    var cleanResponse = fullResponse.ToString().Replace("[END_INTERVIEW]", "").Trim();
    //    session.AddTurn(Speaker.Interviewer, cleanResponse);

    //    if(countAsQuestions)
    //    {
    //        var state = _connections.Get(Context.ConnectionId);
    //        if(state != null)
    //        {
    //            state.QuestionsAsked++;
    //            _logger.LogInformation("QuestionsAsked now {Count}/{Target}", state.QuestionsAsked, state.Session.Configuration.TargetQuestionCount);
    //        }
    //    }

    //    await Clients.Caller.SendAsync("InterviewerTurnComplete", shouldEnd);

    //    if (shouldEnd)
    //    {
    //        var evaluationPrompt = EvaluationPromptBuilder.Build(session);
    //        await Clients.Caller.SendAsync("InterviewComplete", evaluationPrompt);
    //        await Task.Delay(500);
    //        Context.Abort();
    //    }
    //}

    private async Task StreamAndSpeakAsync(InterviewSession session, bool countAsQuestions = true, bool forceEnd = false)
    {
        var sentenceBuffer = new StringBuilder();
        var fullResponse = new StringBuilder();

        const int HardFlushLimit = 350;

        await foreach (var token in _llmService.StreamNextQuestionAsync(session, Context.ConnectionAborted))
        {
            sentenceBuffer.Append(token);
            fullResponse.Append(token);

            if (fullResponse.ToString().Contains("[END_INTERVIEW]"))
            {
                _logger.LogWarning("Model emitted [END_INTERVIEW] on its own — ignoring, model does not control ending.");
            }

            while (true)
            {
                var match = Regex.Match(sentenceBuffer.ToString(), @"^(.*?[.!?])\s+");
                if (!match.Success) break;

                var sentence = match.Groups[1].Value.Replace("[END_INTERVIEW]", "").Trim();
                sentenceBuffer.Remove(0, match.Length);

                if (sentence.Length > 0)
                {
                    var audio = await _ttsService.SynthesizeAsync(sentence);
                    await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
                }
            }

            if (sentenceBuffer.Length >= HardFlushLimit)
            {
                var content = sentenceBuffer.ToString();
                int cutPoint = content.LastIndexOf(' ');
                if (cutPoint <= 0) cutPoint = content.Length;

                var forced = content.Substring(0, cutPoint).Replace("[END_INTERVIEW]", "").Trim();
                sentenceBuffer.Remove(0, cutPoint);

                if (forced.Length > 0)
                {
                    var audio = await _ttsService.SynthesizeAsync(forced);
                    await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", forced, audio.AudioData);
                }
            }
        }

        if (sentenceBuffer.Length > 0)
        {
            var sentence = sentenceBuffer.ToString().Replace("[END_INTERVIEW]", "").Trim();
            if (sentence.Length > 0)
            {
                var audio = await _ttsService.SynthesizeAsync(sentence);
                await Clients.Caller.SendAsync("ReceiveInterviewerAudioChunk", sentence, audio.AudioData);
            }
        }

        var cleanResponse = fullResponse.ToString().Replace("[END_INTERVIEW]", "").Trim();
        session.AddTurn(Speaker.Interviewer, cleanResponse);

        if (countAsQuestions)
        {
            var state = _connections.Get(Context.ConnectionId);
            if (state != null)
            {
                state.QuestionsAsked++;
                _logger.LogInformation("QuestionsAsked now {Count}/{Target}", state.QuestionsAsked, state.Session.Configuration.TargetQuestionCount);
            }
        }

        await Clients.Caller.SendAsync("InterviewerTurnComplete", forceEnd);

        if (forceEnd)
        {
            var evaluationPrompt = EvaluationPromptBuilder.Build(session);
            await Clients.Caller.SendAsync("InterviewComplete", evaluationPrompt);
            await Task.Delay(500);
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

        try
        {
            var pcmBytes = state.AudioBuffer.ToArray();
            state.AudioBuffer.Clear();

            if (pcmBytes.Length == 0)
            {
                _logger.LogWarning("EndOfSpeech called with empty audio buffer for {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("TranscriptionFailed", "No Audio Captured");
                return;
            }

            using var wavStream = ConvertPcmBytesToWavStream(pcmBytes, SampleRate);
            var transcription = await _sttService.TranscribeAudioStreamAsync(wavStream);

            bool isUsable = transcription.Success && !string.IsNullOrWhiteSpace(transcription.Text)
                && transcription.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2;

            if (!isUsable)
            {
                state.ConsecutiveEmptyTurns++;
                _logger.LogWarning("Empty/short transcription ({Count} in a row) for {ConnectionId}", state.ConsecutiveEmptyTurns, Context.ConnectionId);

                await Clients.Caller.SendAsync("TranscriptionFailed", transcription.ErrorMessage ?? "Uncle or empty response");

                if (state.ConsecutiveEmptyTurns >= 3)
                {
                    state.Session.AddTurn(Speaker.Candidate, "[SYSTEM NOTE: Candidate unresponsive after 3 attempts. End the interview now, warmly and briefly.]");
                    await StreamAndSpeakAsync(state.Session, forceEnd: true);
                }
                return;
            }

            state.ConsecutiveEmptyTurns = 0;
            await Clients.Caller.SendAsync("CandidateTranscript", transcription.Text);

            if(state.QuestionsAsked >= state.Session.Configuration.TargetQuestionCount)
            {
                state.Session.AddTurn(Speaker.Candidate, transcription.Text);
                state.Session.AddTurn(Speaker.Candidate, "[SYSTEM NOTE: The target number of questions has been reached. End the interview now, warmly and briefly , per your instruction.]");
                await StreamAndSpeakAsync(state.Session, forceEnd: true);
                return;
            }

            state.Session.AddTurn(Speaker.Candidate, transcription.Text);
            await StreamAndSpeakAsync(state.Session);
        }
        finally
        {
            state.AudioBuffer.Clear();
            state.IsProcessingTurn = false;
        }
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
 