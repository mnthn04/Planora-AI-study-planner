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
    [Route("api/subjects")]
    public class SubjectController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubjectController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
        {
            var subjects = await _unitOfWork.Subjects.GetSubjectsByUserIdAsync(UserId!);
            return Ok(subjects);
        }

        [AllowAnonymous]
        [HttpGet("debug/all")]
        public async Task<IActionResult> DebugAllSubjects()
        {
            try {
                var subjects = await _unitOfWork.Subjects.FindAsync(s => true); // just basic
                var users = await _unitOfWork.Subjects.GetSubjectsByUserIdAsync(subjects.FirstOrDefault()?.UserId ?? "");
                var settings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(users, settings);
                return Content(json, "application/json");
            } catch (Exception ex) {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Subject>> GetSubject(Guid id)
        {
            var subject = await _unitOfWork.Subjects.GetSubjectWithDetailsAsync(id);

            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            return Ok(subject);
        }

        [HttpPost]
        public async Task<ActionResult<Subject>> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                SubjectName = request.SubjectName,
                UserId = UserId!,
                ExamDate = request.ExamDate,
                DailyStudyHours = request.DailyStudyHours
            };

            await _unitOfWork.Subjects.AddAsync(subject);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, subject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] UpdateSubjectRequest request)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id);

            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            subject.SubjectName = request.SubjectName;
            subject.ExamDate = request.ExamDate;
            subject.DailyStudyHours = request.DailyStudyHours;

            _unitOfWork.Subjects.Update(subject);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var subject = await _unitOfWork.Subjects.GetByIdAsync(id);

            if (subject == null || subject.UserId != UserId)
            {
                return NotFound();
            }

            _unitOfWork.Subjects.Remove(subject);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }

    public record CreateSubjectRequest(
        [Required][MinLength(2)][MaxLength(100)] string SubjectName, 
        [Required] DateTime ExamDate, 
        [Required][Range(1, 24)] int DailyStudyHours
    );

    public record UpdateSubjectRequest(
        [Required][MinLength(2)][MaxLength(100)] string SubjectName,
        [Required] DateTime ExamDate,
        [Required][Range(1, 24)] int DailyStudyHours
    );
}
