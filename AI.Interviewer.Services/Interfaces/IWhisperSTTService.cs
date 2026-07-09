using AI.Interviewer.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Interfaces
{
    public interface IWhisperSTTService
    {

        bool IsInitialized { get; }
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task<TranscriptionResult> TranscribeAudioStreamAsync(Stream audioStrream, CancellationToken cancellationToken = default);
    }
}
