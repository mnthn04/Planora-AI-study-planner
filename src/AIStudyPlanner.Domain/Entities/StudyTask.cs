using System;
using System.Collections.Generic;
using AIStudyPlanner.Domain.Enums;

namespace AIStudyPlanner.Domain.Entities
{
    public class StudyTask
    {
        public Guid Id { get; set; }
        public Guid StudyPlanId { get; set; }
        public int DayNumber { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string Activities { get; set; } = string.Empty;
        public StudyTaskStatus Status { get; set; } = StudyTaskStatus.Pending;
        public DateTime? CompletedAt { get; set; }

        public StudyPlan StudyPlan { get; set; } = null!;
        public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
    }
}
