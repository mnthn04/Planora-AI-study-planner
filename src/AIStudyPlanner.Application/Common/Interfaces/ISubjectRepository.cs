using AIStudyPlanner.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIStudyPlanner.Application.Common.Interfaces
{
    public interface ISubjectRepository : IRepository<Subject>
    {
        Task<IEnumerable<Subject>> GetSubjectsByUserIdAsync(string userId);
        Task<Subject?> GetSubjectWithDetailsAsync(Guid id);
        Task<IEnumerable<Subject>> GetSubjectsForAnalyticsAsync(string userId);
    }
}
