using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Options
{
    public class TTSOptions
    {
        public const string SectionName = "TTS";
        public string VoiceName { get; set; } = "af_heart";
        public float Speed { get; set; } = 1.0f;
    }
}
