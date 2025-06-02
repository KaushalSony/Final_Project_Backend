namespace Final_Project_WebAPI.DTO
{
    public class AssessmentReadDTO
    {
        public Guid AssessmentId { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public List<QuestionDTO> Questions { get; set; } = new();
        public int MaxScore { get; set; }
    }

    public class AssessmentCreateDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public List<QuestionDTO> Questions { get; set; } = new();
        public int MaxScore { get; set; }
    }
}
