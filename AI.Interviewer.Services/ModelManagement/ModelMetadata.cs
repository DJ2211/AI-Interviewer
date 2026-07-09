namespace AI.Interviewer.Services.ModelManagement;
public class ModelMetadata
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeeInMB { get; set; }
    public string Description { get; set; } = string.Empty;
}

public static class ModelCatalog
{
    public static readonly ModelMetadata WhisperSmall = new()
    {
        Name = "Whisper Small",
        FileName = "ggml-small-q8_0.bin",
        DownloadUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
        SizeeInMB = 466,
        Description = "Whisper small model for STT (Speech to text) processing"
    };
}

