using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.ModelManagement.Interface
{
    public interface IModelManager
    {
        // Description: Get the model path of the Whisper Model
        public string GetWhisperModelPath();

        // Description: Check and return boolean if the Whisper Model exists
        bool WhisperModelExists();
    }
}
