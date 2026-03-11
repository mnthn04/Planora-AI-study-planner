using System;
using System.Collections.Generic;

namespace AIStudyPlanner.Domain.Entities
{
    public class StudyPlan
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public Subject Subject { get; set; } = null!;
        public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    }
}
