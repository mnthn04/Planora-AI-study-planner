using System;

namespace AIStudyPlanner.Domain.Entities
{
    public class Topic
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = "Medium";
        
        public Subject Subject { get; set; } = null!;
    }
}
