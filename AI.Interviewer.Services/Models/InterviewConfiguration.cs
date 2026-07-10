using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models
{
    public enum DifficultyLevel { Junior, Medium, Senior};
    public enum InterviewerStyle { Friendly, Neutral, Strict}
    public class InterviewConfiguration
    {
        public string Role { get; set; } = "Software Developer";
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public InterviewerStyle Style { get; set; } = InterviewerStyle.Friendly;
        public int TargetQuestionCount { get; set; } = 10;
        public int NoOfQuestions { get; set; } = 20;
        public bool IncludeCodingQuestions { get; set; } = true;
        public string InterviewerName { get; set; } = "Sarah";
        public string? AdditionalInstructions { get; set; }
    }
}
