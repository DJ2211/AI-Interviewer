using AI.Interviewer.Services.Models;
using AI.Interviewer.Api.Hubs;
using AI.Interviewer.Api.State;
using AI.Interviewer.Services;
using AI.Interviewer.Services.Interfaces;

ThreadPool.SetMinThreads(Environment.ProcessorCount * 4, Environment.ProcessorCount * 4);
var builder = WebApplication.CreateBuilder(args);

// --- SignalR ---
builder.Services.AddSignalR();

// --- Your existing service registrations ---
builder.Services.AddWhisperServices(builder.Configuration);
builder.Services.AddOllamaLLMServices(builder.Configuration);
builder.Services.AddKokoroTTSServices(builder.Configuration);

// --- Connection state manager ---
builder.Services.AddSingleton<InterviewConnectionManager>();

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

app.MapHub<InterviewerHub>("/interviewHub");

// --- Initialize all three services once at startup ---
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Initializing Whisper...");
    await scope.ServiceProvider.GetRequiredService<IWhisperSTTService>().InitializeAsync();

    logger.LogInformation("Initializing LLM...");
    await scope.ServiceProvider.GetRequiredService<ILLMInterviewerService>().InitializeAsync();

    logger.LogInformation("Initializing TTS...");
    await scope.ServiceProvider.GetRequiredService<ITTSService>().InitializeAsync();

    logger.LogInformation("All services ready.");
}

app.Run();