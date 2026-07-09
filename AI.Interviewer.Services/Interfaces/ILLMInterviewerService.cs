using AI.Interviewer.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Interfaces
{
    public interface ILLMInterviewerService
    {
        bool IsInitialized { get; }
        /// <summary>
        /// Verifies Ollama is reachable and configured model is available
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Given the current session (with candidate's latest answer already added to History),
        /// Generates the interviewer's next response/question.
        /// </summary>
        Task<string> GetNextQuestionAsync(InterviewSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Token streaming service which will send audio sentence by sentence, instead of full audio
        /// </summary>
        IAsyncEnumerable<string> StreamNextQuestionAsync(InterviewSession session, CancellationToken cancellationToken = default);
    }
}
