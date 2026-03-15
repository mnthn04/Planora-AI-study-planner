using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AIStudyPlanner.Infrastructure.Services
{
    public class GroqStudyPlannerService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GroqStudyPlannerService> _logger;
        private readonly string _apiKey;
        private const string Url = "https://api.groq.com/openai/v1/chat/completions";
        private const string Model = "llama-3.1-8b-instant";

        public GroqStudyPlannerService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqStudyPlannerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GROQ_API_KEY"] 
                      ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
                      ?? configuration["GROQ:ApiKey"];
        }

        public async Task<List<GeminiPlanItem>> GenerateStudyPlanAsync(string subject, List<string> topics, string difficulty, DateTime examDate, int dailyHours)
        {
            _logger.LogInformation("Generating study plan for subject: {Subject} using Groq {Model}", subject, Model);

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Groq API Key is missing. Skipping Groq generation.");
                throw new Exception("Groq API key is missing.");
            }

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
                    5. For each day, provide a 'day' (integer), 'topic' (string), and a list of 3-4 specific 'activities' (array of strings).
                    6. IMPORTANT: Return ONLY a JSON array. DO NOT include any explanatory text before or after the JSON.

                    Format:
                    [
                      {{ 
                        ""day"": 1, 
                        ""topic"": ""Topic Name"", 
                        ""activities"": [
                          ""Activity 1"",
                          ""Activity 2""
                        ]
                      }}
                    ]
                ";

                var requestBody = new
                {
                    model = Model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful study planner that outputs only valid JSON arrays." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.5
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                
                // Clear and set auth header for each request to be safe with shared HttpClient
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                
                var response = await _httpClient.PostAsync(Url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Groq API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string rawText = dynamicResponse.choices[0].message.content;

                return ExtractJsonList<GeminiPlanItem>(rawText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq API failed in GenerateStudyPlanAsync.");
                throw;
            }
        }

        public async Task<List<FlashcardItem>> GenerateFlashcardsAsync(string topic, string activities)
        {
            _logger.LogInformation("Generating flashcards for topic: {Topic} using Groq", topic);
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Groq API key is missing.");
            }

            try
            {
                var prompt = $@"Create 5 flashcards for: {topic}. Activities context: {activities}. Return ONLY JSON array of {{""question"", ""answer""}} objects.";
                var requestBody = new
                {
                    model = Model,
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.7
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                
                var response = await _httpClient.PostAsync(Url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Groq Flashcards API error: {Error}", error);
                    throw new Exception($"Groq Flashcards API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string rawText = dynamicResponse.choices[0].message.content;

                return ExtractJsonList<FlashcardItem>(rawText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq Flashcards generation failed.");
                throw;
            }
        }

        public async Task<List<PracticeTestItem>> GeneratePracticeTestAsync(string subject, List<string> topics)
        {
             _logger.LogInformation("Generating practice test for subject: {Subject} using Groq", subject);
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Groq API key is missing.");
            }

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

                    Format: Return ONLY a JSON array of objects with keys: ""question"", ""options"" (array of 4 strings), ""correctAnswer"" (one of ""A"", ""B"", ""C"", ""D""), and ""explanation"".
                ";

                var requestBody = new
                {
                    model = Model,
                    messages = new[] 
                    { 
                        new { role = "system", content = "You are an educational assistant that generates high-quality practice exams in JSON format." },
                        new { role = "user", content = prompt } 
                    },
                    temperature = 0.6
                };
                
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                
                var response = await _httpClient.PostAsync(Url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Groq Practice Test API error: {Error}", error);
                    throw new Exception($"Groq Practice Test API error: {error}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var dynamicResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string rawText = dynamicResponse.choices[0].message.content;

                return ExtractJsonList<PracticeTestItem>(rawText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq Practice Test generation failed.");
                throw;
            }
        }

        private List<T> ExtractJsonList<T>(string rawText)
        {
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

            return JsonConvert.DeserializeObject<List<T>>(rawText) ?? new List<T>();
        }
    }
}
