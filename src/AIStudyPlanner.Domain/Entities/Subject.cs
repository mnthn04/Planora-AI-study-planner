using System;
using System.Collections.Generic;

namespace AIStudyPlanner.Domain.Entities
{
    public class Subject
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public int DailyStudyHours { get; set; }
        
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public StudyPlan? StudyPlan { get; set; }
    }
}
