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
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
