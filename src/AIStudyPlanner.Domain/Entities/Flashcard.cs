using System;

namespace AIStudyPlanner.Domain.Entities
{
    public class Flashcard
    {
        public Guid Id { get; set; }
        public Guid StudyTaskId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public StudyTask StudyTask { get; set; } = null!;
    }
}
