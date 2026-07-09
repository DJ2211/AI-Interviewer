using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Options
{
    public class LLMOptions
    {
        public const string SectionName = "LLM";
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string ModelName { get; set; } = "llama3.2:3b";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 512;
        public string SystemPrompt { get; set; } = string.Empty;
    }
}
