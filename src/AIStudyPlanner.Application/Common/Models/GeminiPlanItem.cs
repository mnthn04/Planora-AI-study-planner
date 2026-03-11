namespace AIStudyPlanner.Application.Common.Models
{
    public class GeminiPlanItem
    {
        public int Day { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string[] Activities { get; set; } = System.Array.Empty<string>();
    }
}
