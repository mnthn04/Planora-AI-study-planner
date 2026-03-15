using System.Collections.Generic;

namespace AIStudyPlanner.Application.Common.Models
{
    public class PracticeTestItem
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
