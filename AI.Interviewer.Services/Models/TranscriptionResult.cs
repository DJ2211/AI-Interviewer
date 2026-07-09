using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models
{
    public class TranscriptionResult
    {
        public string Text { get; set; } = string.Empty;
        public float Confidence { get; set; } = 0.85f;
        public TimeSpan ProcessingTime { get; set; }
        public DateTime TrancribedAt { get; set; } = DateTime.UtcNow;
        public string Language { get; set; } = "en";
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
