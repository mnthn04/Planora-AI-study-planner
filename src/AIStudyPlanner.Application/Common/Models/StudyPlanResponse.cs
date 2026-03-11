using System.Collections.Generic;

namespace AIStudyPlanner.Application.Common.Models
{
    public class StudyPlanDayResponse
    {
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class StudyPlanResponse
    {
        public List<StudyPlanDayResponse> Days { get; set; } = new List<StudyPlanDayResponse>();
    }
}
