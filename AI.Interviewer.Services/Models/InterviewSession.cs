using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models;
public class InterviewSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public InterviewConfiguration Configuration { get; set; } = new();
    public List<InterviewTurn> History { get; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public void AddTurn(Speaker speaker, string text)
    {
        History.Add(new InterviewTurn { Speaker = speaker, Text = text });
    }
}
