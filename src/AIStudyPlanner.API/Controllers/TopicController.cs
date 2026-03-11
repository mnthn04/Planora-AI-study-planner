using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
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
    [Route("api/topics")]
    public class TopicController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TopicController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("bySubject/{subjectId}")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetTopicsBySubject(Guid subjectId)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId);
            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            var topics = await _unitOfWork.Topics.FindAsync(t => t.SubjectId == subjectId);
            return Ok(topics);
        }

        [HttpPost]
        public async Task<ActionResult<Topic>> CreateTopic([FromBody] CreateTopicRequest request)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
            if (subject == null || subject.UserId != UserId)
            {
                return BadRequest("Invalid Subject ID");
            }

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                SubjectId = request.SubjectId,
                TopicName = request.TopicName,
                DifficultyLevel = request.DifficultyLevel
            };

            await _unitOfWork.Topics.AddAsync(topic);
            await _unitOfWork.CompleteAsync();

            return Ok(topic);
        }
    }

    public record CreateTopicRequest(
        [Required] Guid SubjectId, 
        [Required][MinLength(2)][MaxLength(100)] string TopicName, 
        [Required] string DifficultyLevel
    );
}
