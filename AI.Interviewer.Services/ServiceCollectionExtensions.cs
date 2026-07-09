using AI.Interviewer.Services.Concrete;
using AI.Interviewer.Services.Interfaces;
using AI.Interviewer.Services.ModelManagement;
using AI.Interviewer.Services.ModelManagement.Interface;
using AI.Interviewer.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Interviewer.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Whisper STT services and their dependencies (model manager, options).
    /// Note: this only wires up dependencies — call IWhisperSTTService.InitializeAsync()
    /// explicitly at startup before using it, since initialization is not done in the constructor.
    /// </summary>
    public static IServiceCollection AddWhisperServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WhisperOptions>(configuration.GetSection(WhisperOptions.SectionName));
        services.AddSingleton<IModelManager, ModelManager>();
        services.AddSingleton<IWhisperSTTService, WhisperSTTService>();
        return services;
    }

    /// <summary>
    /// Registers OllamaLLM services and their dependencies (options).
    /// Note : this only wires up dependencies - call ILLMInterviewerService.InitializeAsync()
    /// explicitly at startup before using it, since initialization is not done in the constructor.
    /// </summary>
    public static IServiceCollection AddOllamaLLMServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LLMOptions>(configuration.GetSection(LLMOptions.SectionName));
        services.AddSingleton<ILLMInterviewerService, OllamaInterviewService>();
        return services;
    }

    /// <summary>
    /// Registers TTSServices and their dependencies (options).
    /// Note : this only wires up dependencies - call ITTSService.InitializeAsync()
    /// explicitly at startup before using it, since initialization is not done in the constructor.
    /// </summary>
    public static IServiceCollection AddKokoroTTSServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TTSOptions>(configuration.GetSection(LLMOptions.SectionName));
        services.AddSingleton<ITTSService, KokoroTTSService>();
        return services;
    }
}
