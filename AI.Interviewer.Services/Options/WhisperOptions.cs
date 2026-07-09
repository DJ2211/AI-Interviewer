using System.Runtime.InteropServices;

namespace AI.Interviewer.Services.Options;

public class WhisperOptions
{
    public const string SectionName = "Whisper";
    public int Threads { get; set; } = Environment.ProcessorCount;
    public string Language { get; set; } = "en";
}
