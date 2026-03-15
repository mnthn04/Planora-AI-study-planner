using System;
using System.Collections.Generic;

namespace AIStudyPlanner.Domain.Entities
{
    public class PracticeTest
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<PracticeQuestion> Questions { get; set; } = new List<PracticeQuestion>();
    }
}
