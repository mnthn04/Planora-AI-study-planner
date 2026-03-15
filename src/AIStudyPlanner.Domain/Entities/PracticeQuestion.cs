using System;

namespace AIStudyPlanner.Domain.Entities
{
    public class PracticeQuestion
    {
        public Guid Id { get; set; }
        public Guid PracticeTestId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        
        public PracticeTest PracticeTest { get; set; } = null!;
    }
}
