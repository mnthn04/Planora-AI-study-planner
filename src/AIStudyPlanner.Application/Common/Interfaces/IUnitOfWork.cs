using System;
using AIStudyPlanner.Domain.Entities;
using System.Threading.Tasks;

namespace AIStudyPlanner.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISubjectRepository Subjects { get; }
        IRepository<Topic> Topics { get; }
        IRepository<StudyPlan> StudyPlans { get; }
        IRepository<StudyTask> StudyTasks { get; }
        IRepository<Flashcard> Flashcards { get; }
        Task<int> CompleteAsync();
    }
}
