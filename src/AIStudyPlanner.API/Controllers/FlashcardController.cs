using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIStudyPlanner.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/flashcards")]
    public class FlashcardController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAIService _aiService;

        public FlashcardController(IUnitOfWork unitOfWork, IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
        }

        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<Flashcard>>> GetFlashcardsByTask(Guid taskId)
        {
            var task = await _unitOfWork.StudyTasks.GetByIdAsync(taskId);
            if (task == null) return NotFound("Task not found");

            var flashcards = await _unitOfWork.Flashcards.FindAsync(f => f.StudyTaskId == taskId);
            return Ok(flashcards);
        }

        [HttpPost("generate/{taskId}")]
        public async Task<ActionResult<IEnumerable<Flashcard>>> GenerateFlashcards(Guid taskId)
        {
            // 1. Check if cards already exist
            var existingCards = await _unitOfWork.Flashcards.FindAsync(f => f.StudyTaskId == taskId);
            if (existingCards.Any())
            {
                return Ok(existingCards);
            }

            // 2. Get task details
            var task = await _unitOfWork.StudyTasks.GetByIdAsync(taskId);
            if (task == null) return NotFound("Task not found");

            // 3. Generate using AI
            var flashcardItems = await _aiService.GenerateFlashcardsAsync(task.TopicName, task.Activities);

            // 4. Save to DB
            var flashcards = flashcardItems.Select(item => new Flashcard
            {
                Id = Guid.NewGuid(),
                StudyTaskId = taskId,
                Question = item.Question,
                Answer = item.Answer,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            foreach (var card in flashcards)
            {
                await _unitOfWork.Flashcards.AddAsync(card);
            }

            await _unitOfWork.CompleteAsync();

            return Ok(flashcards);
        }
    }
}
