using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIStudyPlanner.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;

        public AnalyticsController(IUnitOfWork unitOfWork, IApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            var subjects = await _unitOfWork.Subjects.GetSubjectsForAnalyticsAsync(UserId!);
            
            if (!subjects.Any())
            {
                return Ok(new { hasData = false });
            }

            var totalSubjects = subjects.Count();
            var activePlans = subjects.Count(s => s.StudyPlan != null);
            
            var allTasks = subjects
                .Where(s => s.StudyPlan != null)
                .SelectMany(s => s.StudyPlan!.StudyTasks)
                .ToList();

            var completedTasks = allTasks.Count(t => t.Status == StudyTaskStatus.Completed);
            var totalTasksCount = allTasks.Count;
            var overallProgress = totalTasksCount > 0 ? (double)completedTasks / totalTasksCount * 100 : 0;

            // Growth graph data (last 7 days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .Reverse()
                .ToList();

            var taskGrowth = last7Days.Select(date => new
            {
                Date = date.ToString("MMM dd"),
                CompletedCount = allTasks.Count(t => t.Status == StudyTaskStatus.Completed && t.CompletedAt?.Date == date)
            }).ToList();

            var subjectGrowth = last7Days.Select(date => new
            {
                Date = date.ToString("MMM dd"),
                CreatedCount = subjects.Count(s => s.CreatedAt.Date <= date)
            }).ToList();

            var practiceTests = await _context.PracticeTests
                .Where(t => t.UserId == UserId!)
                .ToListAsync();

            var testStats = practiceTests.Any() ? new
            {
                TotalTests = practiceTests.Count,
                AverageScore = Math.Round(practiceTests.Average(t => (double)t.Score / t.TotalQuestions * 100), 1),
                HighestScore = practiceTests.Max(t => (double)t.Score / t.TotalQuestions * 100),
                RecentScores = practiceTests.OrderByDescending(t => t.CreatedAt).Take(5).Select(t => new { t.SubjectName, t.Score, t.TotalQuestions, t.CreatedAt })
            } : null;

            return Ok(new
            {
                hasData = true,
                summary = new
                {
                    totalSubjects,
                    activePlans,
                    completedTasks,
                    totalTasks = totalTasksCount,
                    overallProgress = Math.Round(overallProgress, 1)
                },
                graphs = new
                {
                    taskGrowth,
                    subjectGrowth
                },
                testStats
            });
        }
    }
}
