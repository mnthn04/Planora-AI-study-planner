using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AIStudyPlanner.Infrastructure.Services
{
    public class GeminiStudyPlannerService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiStudyPlannerService> _logger;
        private readonly string _apiKey;
        private const string Url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiStudyPlannerService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiStudyPlannerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] 
                      ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? configuration["GEMINI_API_KEY"]
                      ?? throw new ArgumentNullException("Gemini:ApiKey or GEMINI_API_KEY is missing");
        }

        public async Task<List<GeminiPlanItem>> GenerateStudyPlanAsync(string subject, List<string> topics, string difficulty, DateTime examDate, int dailyHours)
        {
            _logger.LogInformation("Generating study plan for subject: {Subject} using Gemini 2.0 Flash", subject);

            try
            {
                var today = DateTime.UtcNow;
                var totalDaysToExam = (int)(examDate.Date - today.Date).TotalDays;
                if (totalDaysToExam < 1) totalDaysToExam = 1;

                var topicList = string.Join(", ", topics);
                var prompt = $@"
                    Create a structured study schedule in JSON format for the following:
                    Subject: {subject}
                    Topics: {topicList}
                    Difficulty: {difficulty}
                    Today's Date: {today:yyyy-MM-dd}
                    Exam Date: {examDate:yyyy-MM-dd}
                    Total Days Available: {totalDaysToExam} days
                    Daily Hours: {dailyHours}

                    Rules:
                    1. Create a plan spanning EXACTLY {totalDaysToExam} days.
                    2. Focus on harder topics first.
                    3. Allocate revision days near the end.
                    4. Balanced mix of theory and practice.
                    5. For each day, provide a 'topic' and a list of 3-4 specific 'activities'.
                    6. IMPORTANT: Return ONLY a JSON array.

                    Format:
                    [
                      {{ 
                        ""day"": 1, 
                        ""topic"": ""Topic Name"", 
                        ""activities"": [
                          ""Review array creation, data types, indexing, slicing."",
                          ""Practice basic array operations (arithmetic, comparisons)."",
                          ""Understand broadcasting rules with simple examples.""
                        ]
                      }}
                    ]
                ";

                var requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } }
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{Url}?key={_apiKey}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gemini Raw Response: {Response}", jsonResponse);

                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                
                if (dynamicResponse.candidates == null || dynamicResponse.candidates.Count == 0)
                {
                    var reason = dynamicResponse.promptFeedback?.blockReason ?? "Unknown";
                    throw new Exception($"Gemini returned no candidates. Reason: {reason}");
                }

                string rawText = dynamicResponse.candidates[0].content.parts[0].text;
                _logger.LogInformation("Gemini Raw Text: {Text}", rawText);

                // More robust JSON extraction
                var jsonStart = rawText.IndexOf('[');
                var jsonEnd = rawText.LastIndexOf(']');
                
                if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
                {
                    rawText = rawText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
                else if (rawText.Contains("```json"))
                {
                    rawText = rawText.Split("```json")[1].Split("```")[0].Trim();
                }
                else if (rawText.Contains("```"))
                {
                    rawText = rawText.Split("```")[1].Split("```")[0].Trim();
                }

                _logger.LogInformation("Extracted JSON: {Json}", rawText);
                return JsonConvert.DeserializeObject<List<GeminiPlanItem>>(rawText) ?? new List<GeminiPlanItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API failed in GenerateStudyPlanAsync.");
                throw;
            }
        }

        public async Task<List<FlashcardItem>> GenerateFlashcardsAsync(string topic, string activities)
        {
            _logger.LogInformation("Generating flashcards for topic: {Topic}", topic);
            try
            {
                var prompt = $@"Create 5 flashcards for: {topic}. Return ONLY JSON array of {{""question"", ""answer""}} objects.";
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{Url}?key={_apiKey}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini Flashcards API error: {Error}", error);
                    throw new Exception($"Gemini Flashcards API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                
                if (dynamicResponse.candidates == null || dynamicResponse.candidates.Count == 0)
                {
                    _logger.LogWarning("Gemini returned no candidates for flashcards.");
                    throw new Exception("Gemini returned no candidates for flashcards.");
                }

                string rawText = dynamicResponse.candidates[0].content.parts[0].text;
                
                // Extract JSON array
                var jsonStart = rawText.IndexOf('[');
                var jsonEnd = rawText.LastIndexOf(']');
                
                if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
                {
                    rawText = rawText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
                else if (rawText.Contains("```json"))
                {
                    rawText = rawText.Split("```json")[1].Split("```")[0].Trim();
                }

                _logger.LogInformation("Extracted Flashcards JSON: {Json}", rawText);
                return JsonConvert.DeserializeObject<List<FlashcardItem>>(rawText) ?? new List<FlashcardItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini Flashcards generation failed.");
                throw;
            }
        }

        public async Task<List<PracticeTestItem>> GeneratePracticeTestAsync(string subject, List<string> topics)
        {
            Console.WriteLine($"[DEBUG] GeneratePracticeTestAsync: {subject}");
            try
            {
                var topicList = string.Join(", ", topics);
                var prompt = $@"
                    Generate 10 multiple-choice questions (MCQs) for the subject '{subject}' covering these topics: {topicList}.
                    
                    For each question:
                    1. Provide a clear and challenging question.
                    2. Provide 4 distinct options.
                    3. Identify the correct answer (must be one of the option letters: 'A', 'B', 'C', or 'D').
                    4. Provide a detailed explanation of why the answer is correct.

                    Rules:
                    - Return ONLY a JSON array.
                    - Do not include any introductory or concluding text.
                    - Ensure the JSON is valid and follows this structure:
                    [
                      {{
                        ""question"": ""What is the main purpose of polymorphism in OOP?"",
                        ""options"": [
                          ""To reduce memory usage"",
                          ""To allow objects to take multiple forms"",
                          ""To hide internal implementation details"",
                          ""To speed up execution time""
                        ],
                        ""correctAnswer"": ""B"",
                        ""explanation"": ""Polymorphism allows objects of different classes to be treated as objects of a common superclass, enabling a single interface to represent different underlying forms.""
                      }}
                    ]
                ";

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                
                Console.WriteLine("[DEBUG] Posting to Gemini...");
                var response = await _httpClient.PostAsync($"{Url}?key={_apiKey}", content);
                Console.WriteLine($"[DEBUG] Response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini Practice Test API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (dynamicResponse.candidates == null || dynamicResponse.candidates.Count == 0)
                {
                    throw new Exception("Gemini returned no candidates for practice test.");
                }

                string rawText = dynamicResponse.candidates[0].content.parts[0].text;
                _logger.LogInformation("Gemini Practice Test Raw Text: {Text}", rawText);

                // Robust JSON extraction
                var jsonStart = rawText.IndexOf('[');
                var jsonEnd = rawText.LastIndexOf(']');
                
                if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
                {
                    rawText = rawText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
                else if (rawText.Contains("```json"))
                {
                    rawText = rawText.Split("```json")[1].Split("```")[0].Trim();
                }

                return JsonConvert.DeserializeObject<List<PracticeTestItem>>(rawText) ?? new List<PracticeTestItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Catch: {ex.Message}");
                throw;
            }
        }
    }
}
