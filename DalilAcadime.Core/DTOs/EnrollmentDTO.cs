using static DalilAcadime.Core.Entities.Enrollment;

namespace DalilAcadime.Core.DTOs
{
    public record CreateEnrollmentDto(
        int StudentId,
        int CourseId,
        EnrollmentStatus EnrollmentStatus
    );

    public record UpdateEnrollmentDto(
        int StudentId,
        int CourseId,
        EnrollmentStatus EnrollmentStatus
    );

    public record EnrollmentResponseDto(
        int Id,
        int StudentId,
        int CourseId,
        EnrollmentStatus EnrollmentStatus,
        DateTime? EnrollmentAt
    );
}
