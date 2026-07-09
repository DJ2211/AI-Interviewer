using AI.Interviewer.Services.Models;

namespace AI.Interviewer.Api.State;

public class InterviewConnectionState
{
    public InterviewSession Session { get; set; } = default!;
    public List<byte> AudioBuffer { get; } = new();
    public int ConsecutiveEmptyTurns { get; set; } = 0;
    public bool IsProcessingTurn { get; set; } = false;
}
