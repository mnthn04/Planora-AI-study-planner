using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Application.Common.Models;
using AIStudyPlanner.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIStudyPlanner.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PracticeTestController : BaseController
    {
        private readonly IAIService _aiService;
        private readonly IApplicationDbContext _context;

        public PracticeTestController(IAIService aiService, IApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        [HttpGet("generate/{subjectId}")]
        public async Task<IActionResult> GenerateTest(Guid subjectId)
        {
            Console.WriteLine($"[DEBUG] Controller: GenerateTest CALLED for subjectId: {subjectId}");
            var subject = await _context.Subjects
                .Include(s => s.Topics)
                .FirstOrDefaultAsync(s => s.Id == subjectId && s.UserId == UserId);

            if (subject == null) return NotFound("Subject not found");

            var topics = subject.Topics.Select(t => t.TopicName).ToList();
            if (!topics.Any()) topics.Add(subject.SubjectName);

            var questions = await _aiService.GeneratePracticeTestAsync(subject.SubjectName, topics);

            return Ok(questions);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTest([FromBody] SubmitTestRequest request)
        {
            var subject = await _context.Subjects.FindAsync(request.SubjectId);
            if (subject == null) return NotFound("Subject not found");

            var practiceTest = new PracticeTest
            {
                Id = Guid.NewGuid(),
                UserId = UserId!,
                SubjectId = request.SubjectId,
                SubjectName = subject.SubjectName,
                Score = request.Score,
                TotalQuestions = request.TotalQuestions,
                CreatedAt = DateTime.UtcNow,
                Questions = request.Questions.Select(q => new PracticeQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = q.QuestionText,
                    OptionA = q.Options[0],
                    OptionB = q.Options[1],
                    OptionC = q.Options[2],
                    OptionD = q.Options[3],
                    CorrectAnswer = q.CorrectAnswer,
                    UserAnswer = q.UserAnswer,
                    Explanation = q.Explanation
                }).ToList()
            };

            _context.PracticeTests.Add(practiceTest);
            await _context.SaveChangesAsync(default);

            return Ok(new { testId = practiceTest.Id, score = practiceTest.Score });
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.PracticeTests
                .Where(t => t.UserId == UserId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.SubjectName,
                    t.Score,
                    t.TotalQuestions,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }

    public class SubmitTestRequest
    {
        public Guid SubjectId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionSubmission> Questions { get; set; } = new List<QuestionSubmission>();
    }

    public class QuestionSubmission
    {
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
