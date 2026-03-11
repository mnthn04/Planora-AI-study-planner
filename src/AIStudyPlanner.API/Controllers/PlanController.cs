using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AIStudyPlanner.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/studyplan")]
    public class PlanController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public PlanController(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        [HttpPost("generate/{subjectId}")]
        public async Task<IActionResult> GeneratePlan(Guid subjectId, [FromBody] GeneratePlanRequest request)
        {
            var subject = await _unitOfWork.Subjects.GetSubjectWithDetailsAsync(subjectId);

            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            if (!subject.Topics.Any())
            {
                return BadRequest("Cannot generate a plan for a subject without topics.");
            }

            var topicsWithDifficulty = subject.Topics
                .Select(t => $"{t.TopicName} ({t.DifficultyLevel})")
                .ToList();

            var planResponse = await _aiService.GenerateStudyPlanAsync(
                subject.SubjectName, 
                topicsWithDifficulty, 
                request.GlobalDifficulty, 
                subject.ExamDate, 
                subject.DailyStudyHours);

            var studyPlan = new StudyPlan
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                CreatedDate = DateTime.UtcNow,
                StudyTasks = planResponse.Select(d => new StudyTask
                {
                    Id = Guid.NewGuid(),
                    DayNumber = d.Day,
                    TopicName = d.Topic,
                    Activities = string.Join("\n", d.Activities),
                    Status = StudyTaskStatus.Pending
                }).ToList()
            };

            // Remove existing plan if any
            if (subject.StudyPlan != null)
            {
                _unitOfWork.StudyPlans.Remove(subject.StudyPlan);
            }

            await _unitOfWork.StudyPlans.AddAsync(studyPlan);
            await _unitOfWork.CompleteAsync();

            return Ok(studyPlan);
        }

        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetPlanBySubject(Guid subjectId)
        {
            var subject = await _unitOfWork.Subjects.GetSubjectWithDetailsAsync(subjectId);
            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            if (subject.StudyPlan == null)
            {
                return NotFound("No study plan generated for this subject.");
            }

            return Ok(subject.StudyPlan);
        }
    }

    public record GeneratePlanRequest([Required] string GlobalDifficulty);
}
