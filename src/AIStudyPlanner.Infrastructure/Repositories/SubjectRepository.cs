using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIStudyPlanner.Infrastructure.Repositories
{
    public class SubjectRepository : Repository<Subject>, ISubjectRepository
    {
        public SubjectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Subject>> GetSubjectsByUserIdAsync(string userId)
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Include(s => s.Topics)
                .Include(s => s.StudyPlan)
                .ThenInclude(p => p.StudyTasks)
                .ToListAsync();
        }

        public async Task<Subject?> GetSubjectWithDetailsAsync(Guid id)
        {
            return await _context.Subjects
                .Include(s => s.Topics)
                .Include(s => s.StudyPlan)
                .ThenInclude(p => p.StudyTasks)
                .ThenInclude(t => t.Flashcards)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Subject>> GetSubjectsForAnalyticsAsync(string userId)
        {
            return await _context.Subjects
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Include(s => s.StudyPlan)
                .ThenInclude(p => p.StudyTasks)
                .Select(s => new Subject
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    CreatedAt = s.CreatedAt,
                    SubjectName = s.SubjectName,
                    StudyPlan = s.StudyPlan != null ? new StudyPlan
                    {
                        Id = s.StudyPlan.Id,
                        StudyTasks = s.StudyPlan.StudyTasks.Select(t => new StudyTask
                        {
                            Status = t.Status,
                            CompletedAt = t.CompletedAt
                        }).ToList()
                    } : null
                })
                .ToListAsync();
        }
    }
}
