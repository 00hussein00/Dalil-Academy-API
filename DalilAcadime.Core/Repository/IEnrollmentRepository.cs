using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;

namespace DalilAcadime.Core.Repository
{
    public interface IEnrollmentRepository
    {
        Task<List<CourseResponseDto>> GetStudentCoursesByUserId(int userId);
        Task<List<EnrollmentResponseDto>> GetAll();
        Task<EnrollmentResponseDto?> GetById(int id);
        Task<EnrollmentResponseDto> Create(Enrollment enrollment);
        Task<EnrollmentResponseDto?> Update(int id, int studentId, int courseId);
        Task<EnrollmentResponseDto?> Delete(int id);
    }
}
