using AI.Interviewer.Console;
using AI.Interviewer.Services;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddWhisperServices(configuration);
services.AddOllamaLLMServices(configuration);
services.AddKokoroTTSServices(configuration);

await using var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ServiceLogger");
var sttService = serviceProvider.GetRequiredService<IWhisperSTTService>();
var llmService = serviceProvider.GetRequiredService<ILLMInterviewerService>();
var ttsService = serviceProvider.GetRequiredService<ITTSService>();

// --- Initialize all three ---
try
{
    logger.LogInformation("Initializing Whisper...");
    await sttService.InitializeAsync();

    logger.LogInformation("Initializing LLM...");
    await llmService.InitializeAsync();

    logger.LogInformation("Initializing Kokoro TTS...");
    await ttsService.InitializeAsync();

    logger.LogInformation("All services initialized.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Service initialization failed");
    return;
}

// --- Build interview session ---
var config = new InterviewConfiguration
{
    Role = ".NET Developer",
    Difficulty = DifficultyLevel.Medium,
    Style = InterviewerStyle.Friendly,
    DurationMinutes = 30,
    IncludeCodingQuestions = false,
    InterviewerName = "Sarah"
};

var session = new InterviewSession { Configuration = config };

// --- Turn 0: AI greeting ---
var greeting = await llmService.GetNextQuestionAsync(session);
Console.WriteLine($"\nSarah: {greeting}\n");
session.AddTurn(Speaker.Interviewer, greeting);

var greetingAudio = await ttsService.SynthesizeAsync(greeting);
if (greetingAudio.Success)
{
    await File.WriteAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "turn-0-greeting.wav"), greetingAudio.AudioData);
    logger.LogInformation("Saved greeting audio ({Time:F2}s)", greetingAudio.ProcessingTime.TotalSeconds);
}

// --- Turn 1: real transcribed audio as candidate's answer ---
string testAudioPath = Path.Combine(AppContext.BaseDirectory, "test-audio.mp3");

if (File.Exists(testAudioPath))
{
    using var audioStream = ConvertToWav.GetWhisperCompatibleWavStream(testAudioPath);
    var transcription = await sttService.TranscribeAudioStreamAsync(audioStream);

    if (transcription.Success)
    {
        Console.WriteLine($"Candidate (transcribed): {transcription.Text}\n");
        session.AddTurn(Speaker.Candidate, transcription.Text);

        var nextQuestion = await llmService.GetNextQuestionAsync(session);
        Console.WriteLine($"Sarah: {nextQuestion}\n");
        session.AddTurn(Speaker.Interviewer, nextQuestion);

        var questionAudio = await ttsService.SynthesizeAsync(nextQuestion);
        if (questionAudio.Success)
        {
            await File.WriteAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "turn-1-question.wav"), questionAudio.AudioData);
            logger.LogInformation("Saved turn-1 audio ({Time:F2}s)", questionAudio.ProcessingTime.TotalSeconds);
        }
    }
    else
    {
        logger.LogWarning("Transcription failed: {Error}", transcription.ErrorMessage);
    }
}
else
{
    logger.LogWarning("test-audio.mp3 not found, skipping audio turn");
}

// --- Remaining turns: type answers, hear + save each response ---
Console.WriteLine("--- Type candidate answers (type 'exit' to stop) ---\n");

int turnIndex = 2;
while (true)
{
    Console.Write("Candidate: ");
    var typedAnswer = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(typedAnswer) || typedAnswer.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    session.AddTurn(Speaker.Candidate, typedAnswer);

    var response = await llmService.GetNextQuestionAsync(session);
    Console.WriteLine($"\nSarah: {response}\n");
    session.AddTurn(Speaker.Interviewer, response);

    var responseAudio = await ttsService.SynthesizeAsync(response);
    if (responseAudio.Success)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"turn-{turnIndex}-question.wav");
        await File.WriteAllBytesAsync(path, responseAudio.AudioData);
        logger.LogInformation("Saved {Path} ({Time:F2}s)", Path.GetFileName(path), responseAudio.ProcessingTime.TotalSeconds);
    }

    turnIndex++;
}

logger.LogInformation("=== Interview ended. Total turns: {TurnCount} ===", session.History.Count);