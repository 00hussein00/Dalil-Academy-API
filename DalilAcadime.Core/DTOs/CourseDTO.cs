using static DalilAcadime.Core.Entities.Course;

namespace DalilAcadime.Core.DTOs
{
    /*
    public class CourseDTO
    {
        public String Title { get; init; }
        public String Price { get; init; }
        public int InstructorId { get; init; }

    }*/

    public record CreateCourseDto(
        string Title,
        float Price,
        CourseLevel Level,
        int InstructorId,
        string? ImageUrl
    );
    public record UpdateCourseDto(
        string Title,
        float Price,
        CourseLevel Level,
        int InstructorId,
        string? ImageUrl

    );
    public record CourseResponseDto
    (
        int Id,
        string Title,
        float Price,
        CourseLevel Level,
        string? InstructorName,
        string? ImageUrl


    );
}
