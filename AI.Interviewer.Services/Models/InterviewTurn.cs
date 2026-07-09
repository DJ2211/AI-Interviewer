using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models
{
    public enum Speaker
    {
        Interviewer,
        Candidate
    }

    public class InterviewTurn
    {
        public Speaker Speaker { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
