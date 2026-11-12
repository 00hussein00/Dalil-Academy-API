namespace DalilAcadime.Core.DTOs
{
    public record CreateEnrollmentDto(
        int StudentId,
        int CourseId
    );

    public record UpdateEnrollmentDto(
        int StudentId,
        int CourseId
    );

    public record EnrollmentResponseDto(
        int Id,
        int StudentId,
        int CourseId,
        DateTime? EnrollmentAt
    );
}
