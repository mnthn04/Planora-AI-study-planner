using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIStudyPlanner.Application.Common.Models;

namespace AIStudyPlanner.Application.Common.Interfaces
{
    public interface IAIService
    {
        Task<List<GeminiPlanItem>> GenerateStudyPlanAsync(string subject, List<string> topics, string difficulty, DateTime examDate, int dailyHours);
        Task<List<FlashcardItem>> GenerateFlashcardsAsync(string topic, string activities);
        Task<List<PracticeTestItem>> GeneratePracticeTestAsync(string subject, List<string> topics);
    }
}
