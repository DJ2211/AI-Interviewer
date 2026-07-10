using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Models
{
    public static class EvaluationPromptBuilder
    {
        public static string Build(InterviewSession session)
        {
            var transcript = new StringBuilder();
            foreach(var turn in session.History)
            {
                transcript.AppendLine($"{(turn.Speaker == Speaker.Interviewer ? "Interviewer" : "Candidate")}: {turn.Text}");
            }

            return $"""
                You are a senior technical interviewer at a top-tier ("FAANG"-level) tech company, reviewing the following interview transcript for a {session.Configuration.Role} position ({session.Configuration.Difficulty} level).

                IMPORTANT CONTEXT: This transcript was produced via speech-to-text and text-to-speech. Minor transcript errors are expected - e.g., misheard names, homophones, or slightly garbled words (e.g a name transcribed incorrectly). Do not penalize the candidate for these kind of speech-to-text artifacts. Only evaluate actual technical/communication substance.

                Evaluate the candidate using the standard 5-tier hiring scale used at major tech companies:
                - Strong Hire
                - Hire
                - Lean Hire
                - Lean No Hire
                - No Hire

                Provide your evaluation in this exact structure: 

                1. **Overall Verdict**: (one of the 5 tiers above)
                2. **Overall Score**: X/100
                3. **Technical Assessment**: strengths and weaknesses in technical answers specifically
                4. **Communication/Non-Technical Assessment**: clarity, structure, confidence, English fluency (ignoring STT artifacts)
                5. **Technical Improvements Needed**: specific, actionable topics to study/practice
                6. **Non-Technical Improvements Needed**: specific, actionable communication/soft-skill suggestions
                7. **Summary**: 2-3 sentence overall impression

                --- TRANSCRIPT ---
                {transcript}
                --- END TRANSCRIPT ---
                """;
        }
    }
}