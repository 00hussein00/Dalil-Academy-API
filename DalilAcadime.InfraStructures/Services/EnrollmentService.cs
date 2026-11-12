using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Services;
using FluentValidation;

namespace DalilAcadime.InfraStructures.Services
{
    public class EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IValidator<CreateEnrollmentDto> createValidator,
        IValidator<UpdateEnrollmentDto> updateValidator) : IEnrollmentService
    {
        public async Task<List<CourseResponseDto>> GetStudentCoursesByUserId(int userId)
        {
            return await enrollmentRepository.GetStudentCoursesByUserId(userId);
        }

        public Task<List<EnrollmentResponseDto>> GetAll()
        {
            return enrollmentRepository.GetAll();
        }

        public async Task<EnrollmentResponseDto?> GetById(int id)
        {
            return await enrollmentRepository.GetById(id)
                ?? throw new NotFoundException($"Enrollment with ID {id} not found.");
        }

        public async Task<EnrollmentResponseDto> Create(CreateEnrollmentDto dto)
        {
            await createValidator.ValidateAndThrowAsync(dto);

            return await enrollmentRepository.Create(new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId
            });
        }

        public async Task<EnrollmentResponseDto?> Update(int id, UpdateEnrollmentDto dto)
        {
            await updateValidator.ValidateAndThrowAsync(dto);

            return await enrollmentRepository.Update(id, dto.StudentId, dto.CourseId)
                ?? throw new NotFoundException($"Enrollment with ID {id} not found.");
        }

        public async Task<EnrollmentResponseDto?> Delete(int id)
        {
            return await enrollmentRepository.Delete(id)
                ?? throw new NotFoundException($"Enrollment with ID {id} not found.");
        }
    }
}
