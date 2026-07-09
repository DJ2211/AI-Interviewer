using AI.Interviewer.Services.ModelManagement.Interface;
using Microsoft.Extensions.Logging;

namespace AI.Interviewer.Services.ModelManagement
{
    public class ModelManager : IModelManager
    {
        private readonly string _modelDirectory;
        private readonly ILogger<ModelManager>? _logger;

        // Constructor
        public ModelManager(ILogger<ModelManager>? logger = null)
        {
            _modelDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".whisper-models"
            );

            _logger = logger;
            Directory.CreateDirectory(_modelDirectory);
        }

        /// <summary>
        /// Gets the full path to the Whisper Small model
        /// </summary>
        public string GetWhisperModelPath()
        {
            string modelPath = Path.Combine(_modelDirectory, ModelCatalog.WhisperSmall.FileName);
            if(!File.Exists(modelPath))
            {
                _logger?.LogError("Whisper Model not Found: {ModelPath}", modelPath);
                throw new FileNotFoundException(
                    $"Whisper model not found: {modelPath}\n\n" + 
                    $"Download from: {ModelCatalog.WhisperSmall.DownloadUrl}\n" + 
                    $"Size Approx: {ModelCatalog.WhisperSmall.SizeeInMB} MB\n" + 
                    $"Save to: {modelPath}"
                );
            }

            _logger?.LogInformation("Whisper model loaded: {ModelPath}", modelPath);
            return modelPath;
        }

        /// <summary>
        /// Checks if the Whisper Small model exists locally
        /// </summary>
        public bool WhisperModelExists()
        {
            string modelPath = Path.Combine(_modelDirectory, ModelCatalog.WhisperSmall.FileName);
            return File.Exists(modelPath);
        }
    }
}
