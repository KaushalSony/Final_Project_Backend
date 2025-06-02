
namespace Final_Project_WebAPI.DTO
{
    public class CourseReadDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid InstructorId { get; set; }
        public string? MediaUrl { get; set; }
    }
    public class CourseCreateDTO
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid InstructorId { get; set; }
        public string? MediaUrl { get; set; }
    }
}
