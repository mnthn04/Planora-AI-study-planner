using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace AIStudyPlanner.Infrastructure.Services
{
    public class FallbackAIService : IAIService
    {
        private readonly GeminiStudyPlannerService _geminiService;
        private readonly GroqStudyPlannerService _groqService;
        private readonly ILogger<FallbackAIService> _logger;

        public FallbackAIService(
            GeminiStudyPlannerService geminiService,
            GroqStudyPlannerService groqService,
            ILogger<FallbackAIService> logger)
        {
            _geminiService = geminiService;
            _groqService = groqService;
            _logger = logger;
        }

        public async Task<List<GeminiPlanItem>> GenerateStudyPlanAsync(string subject, List<string> topics, string difficulty, DateTime examDate, int dailyHours)
        {
            try
            {
                return await _geminiService.GenerateStudyPlanAsync(subject, topics, difficulty, examDate, dailyHours);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini failed to generate study plan. Falling back to Groq.");
                try
                {
                    return await _groqService.GenerateStudyPlanAsync(subject, topics, difficulty, examDate, dailyHours);
                }
                catch (Exception groqEx)
                {
                    _logger.LogError(groqEx, "Groq also failed to generate study plan. Returning basic initial plan.");
                    return new List<GeminiPlanItem>
                    {
                        new GeminiPlanItem 
                        { 
                            Day = 1, 
                            Topic = "Intro to " + subject, 
                            Activities = new[] { "Setup your study environment", "Review basic concepts and terminology" } 
                        }
                    };
                }
            }
        }

        public async Task<List<FlashcardItem>> GenerateFlashcardsAsync(string topic, string activities)
        {
            try
            {
                return await _geminiService.GenerateFlashcardsAsync(topic, activities);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini failed to generate flashcards. Falling back to Groq.");
                try
                {
                    return await _groqService.GenerateFlashcardsAsync(topic, activities);
                }
                catch (Exception groqEx)
                {
                    _logger.LogError(groqEx, "Groq also failed to generate flashcards.");
                    return new List<FlashcardItem> 
                    { 
                        new FlashcardItem { Question = "API Error", Answer = "Could not generate flashcards at this time. Please try again later." } 
                    };
                }
            }
        }

        public async Task<List<PracticeTestItem>> GeneratePracticeTestAsync(string subject, List<string> topics)
        {
            try
            {
                return await _geminiService.GeneratePracticeTestAsync(subject, topics);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini failed to generate practice test. Falling back to Groq.");
                try
                {
                    return await _groqService.GeneratePracticeTestAsync(subject, topics);
                }
                catch (Exception groqEx)
                {
                    _logger.LogError(groqEx, "Groq also failed to generate practice test.");
                    return new List<PracticeTestItem> { 
                        new PracticeTestItem { Question = "Could not generate questions due to API errors", Options = new List<string>{"N/A","N/A","N/A","N/A"}, CorrectAnswer="N/A", Explanation="Check API logs" }
                    };
                }
            }
        }
    }
}
