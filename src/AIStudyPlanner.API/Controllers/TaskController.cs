using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AIStudyPlanner.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
        {
            var task = await _unitOfWork.StudyTasks.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Optional: verify ownership through StudyPlan -> Subject -> UserId
            // For now, simplicity
            
            if (Enum.TryParse<StudyTaskStatus>(request.Status, out var status))
            {
                task.Status = status;
                await _unitOfWork.CompleteAsync();
                return Ok(task);
            }

            return BadRequest("Invalid status");
        }
    }

    public record UpdateTaskStatusRequest(string Status);
}
