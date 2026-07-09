using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models
{
    public class SynthesisResult
    {
        public byte[] AudioData { get; set; } = Array.Empty<byte>();
        public TimeSpan ProcessingTime { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
