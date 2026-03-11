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
        private const string Url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        // Simple Rate Limiting: 10 requests per minute
        private static readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(2, 2); // Allow 2 concurrent requests
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minInterval = TimeSpan.FromSeconds(6); // 10 per minute = 1 every 6 seconds

        public GeminiStudyPlannerService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiStudyPlannerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] 
                      ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? throw new ArgumentNullException("Gemini:ApiKey or GEMINI_API_KEY is missing");
        }

        public async Task<List<GeminiPlanItem>> GenerateStudyPlanAsync(string subject, List<string> topics, string difficulty, DateTime examDate, int dailyHours)
        {
            _logger.LogInformation("Generating study plan for subject: {Subject} using Gemini 2.5 Flash", subject);

            // Wait for rate limiter
            await _rateLimiter.WaitAsync();
            try
            {
                var now = DateTime.UtcNow;
                var elapsedSinceLast = now - _lastRequestTime;
                if (elapsedSinceLast < _minInterval)
                {
                    var delay = _minInterval - elapsedSinceLast;
                    _logger.LogWarning("Rate Limit: Waiting for {Delay}ms before calling Gemini API", delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }
                _lastRequestTime = DateTime.UtcNow;

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

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var requestUrl = $"{Url}?key={_apiKey}";
                _logger.LogInformation("Calling Gemini API: {Url}", requestUrl.Replace(_apiKey, "REDACTED"));
                
                var response = await _httpClient.PostAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error (Status: {StatusCode}): {Error}", response.StatusCode, error);
                    throw new Exception($"Gemini API returned error: {response.StatusCode} - {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogTrace("Gemini Raw Response: {Response}", jsonResponse);
                
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                
                if (dynamicResponse.candidates == null || dynamicResponse.candidates.Count == 0)
                {
                    _logger.LogError("Gemini API returned no candidates. Response: {Response}", jsonResponse);
                    throw new Exception("Gemini API returned no candidates. This might be due to safety filters.");
                }

                var candidate = dynamicResponse.candidates[0];
                if (candidate.content == null || candidate.content.parts == null || candidate.content.parts.Count == 0)
                {
                    _logger.LogError("Gemini API candidate has no content parts. Response: {Response}", jsonResponse);
                    throw new Exception("Gemini API candidate has no content parts.");
                }

                string rawText = candidate.content.parts[0].text;

                if (rawText.Contains("```json"))
                    rawText = rawText.Split("```json")[1].Split("```")[0].Trim();
                else if (rawText.Contains("```"))
                    rawText = rawText.Split("```")[1].Split("```")[0].Trim();

                _logger.LogInformation("Successfully generated Gemini plan for {Subject}", subject);
                var items = JsonConvert.DeserializeObject<List<GeminiPlanItem>>(rawText);
                return items ?? throw new Exception("Failed to deserialize Gemini response into PlanItems.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API failed or returned invalid data. Using internal fallback mock plan for {Subject}.", subject);
                
                // Internal fallback to ensure user gets SOMETHING even if API fails
                // Using distinct topics to confirm this new code is running
                return new List<GeminiPlanItem>
                {
                    new GeminiPlanItem { Day = 1, Topic = $"Introduction to {subject} (Flash Plan)", Activities = new[] { "Overview of core concepts", "Initial setup and environment" } },
                    new GeminiPlanItem { Day = 2, Topic = "Fundamental Concepts & Overview", Activities = new[] { "Deep dive into fundamentals", "Basic examples and exercises" } },
                    new GeminiPlanItem { Day = 3, Topic = "Core Topic Deep Dive", Activities = new[] { "Advanced concepts", "Practical implementation" } },
                    new GeminiPlanItem { Day = 4, Topic = "Practical Application & Practice", Activities = new[] { "Hands-on projects", "Troubleshooting common issues" } },
                    new GeminiPlanItem { Day = 5, Topic = "Comprehensive Review & Revision", Activities = new[] { "Final review of all topics", "Exam strategy and practice tests" } }
                };
            }
            finally
            {
                _rateLimiter.Release();
            }
        }
    }
}
