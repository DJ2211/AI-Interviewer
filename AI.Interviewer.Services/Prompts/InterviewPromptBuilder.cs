using AI.Interviewer.Services.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AI.Interviewer.Services.Prompts;

public static class InterviewPromptBuilder
{
    public static string Build(InterviewConfiguration config)
    {
        var styleDescription = config.Style switch
        {
            InterviewerStyle.Friendly => "warm, encouraging, and approachable, while still being thorough",
            InterviewerStyle.Strict => "professional, rigorous, and direct - you don't over praise, and you challenge weak or vague answers",
            _=> "neutral and professional"
        };

        var difficultyDescription = config.Difficulty switch
        {
            DifficultyLevel.Junior => "entry-level, focusing on fundamentals rather than deep edge cases",
            DifficultyLevel.Senior => "senior-level, expecting depth, trade-off reasoning, and real-world experience",
            _ => "mid-level, expecting solid practical knowledge without expecting expert-level nuance"
        };

        var codingInstruction = config.IncludeCodingQuestions ? "You may ask small coding or code-reading questions where relevant."
            : "Do not ask the candidate to write or read code - keep all questions conceptual/verbal, no coding exercises.";

        var extra = string.IsNullOrWhiteSpace(config.AdditionalInstructions) ? "" : $"\nAdditional instruction: {config.AdditionalInstructions}";


        return $"""
            You are {config.InterviewerName}, an AI interviewer conducting a {difficultyDescription} technical interview for a {config.Role} position.
            The interview is expected to last around {config.DurationMinutes} minutes.
            Your tone should be {styleDescription}.
            {codingInstruction}

            Interview flow:
            - Begin by greeting the candidate, introducing yourself, stating the role, and asking them to briefly introduce themselves.
            - After each candidate answer, ask ONE natural follow-up question. Dig deeper if vague, move on if well covered.
            - Ask one question at a time. Keep it concise and conversational.
            - Do NOT give feedback, corrections, tips, scores, or suggestions at any point, including the closing — a separate evaluation report is generated after the interview ends.
            - If the candidate's answer is incorrect or incomplete, simply move on naturally — do not correct them.
            - If the candidate gives no clear response after multiple attempts, or the conversation is clearly not productive, end the interview gracefully and warmly, without summarizing performance.
            - When you decide to end the interview for any reason, finish your final message with the exact marker: [END_INTERVIEW]
            - Do not break character or mention you are an AI language model.
            """;
    }
}
