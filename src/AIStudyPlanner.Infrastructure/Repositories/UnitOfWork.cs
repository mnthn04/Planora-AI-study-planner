using System;
using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace AIStudyPlanner.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Subjects = new SubjectRepository(_context);
            Topics = new Repository<Topic>(_context);
            StudyPlans = new Repository<StudyPlan>(_context);
            StudyTasks = new Repository<StudyTask>(_context);
            Flashcards = new Repository<Flashcard>(_context);
        }

        public ISubjectRepository Subjects { get; private set; }
        public IRepository<Topic> Topics { get; private set; }
        public IRepository<StudyPlan> StudyPlans { get; private set; }
        public IRepository<StudyTask> StudyTasks { get; private set; }
        public IRepository<Flashcard> Flashcards { get; private set; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
