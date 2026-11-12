using DalilAcadime.Core.DTOs;

namespace DalilAcadime.Core.Services
{
    public interface IEnrollmentService
    {
        Task<List<CourseResponseDto>> GetStudentCoursesByUserId(int userId);
        Task<List<EnrollmentResponseDto>> GetAll();
        Task<EnrollmentResponseDto?> GetById(int id);
        Task<EnrollmentResponseDto> Create(CreateEnrollmentDto dto);
        Task<EnrollmentResponseDto?> Update(int id, UpdateEnrollmentDto dto);
        Task<EnrollmentResponseDto?> Delete(int id);
    }
}
