using AIStudyPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AIStudyPlanner.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Subject> Subjects { get; }
        DbSet<Topic> Topics { get; }
        DbSet<StudyPlan> StudyPlans { get; }
        DbSet<StudyTask> StudyTasks { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
