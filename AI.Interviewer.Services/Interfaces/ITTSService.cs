using AI.Interviewer.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Interfaces
{
    public interface ITTSService
    {
        bool IsInitialized { get; }
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task<SynthesisResult> SynthesizeAsync(string text, CancellationToken cancellationToken = default);
    }
}
