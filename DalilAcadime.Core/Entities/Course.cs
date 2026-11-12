namespace DalilAcadime.Core.Entities
{
    public class Course
    {
        public enum CourseLevel
        {
            Beginner,
            Intermediate,
            Advanced
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public float Price { get; set; }

        public CourseLevel Level { get; set; }

        // relationship with Instructor
        public int InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
        public string? ImageUrl { get; set; } 
        // navigation property for the relationship with Enrollment
        public List<Enrollment> Enrollments { get; set; } = [];
    }
}
