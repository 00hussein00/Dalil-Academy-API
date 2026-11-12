using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Mappings;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Security;
using DalilAcadime.Core.Services;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Services
{
    public class StudentService(
        IStudentRepository studentRepository,
        IValidator<CreateStudentDto> createValidator,
        IValidator<UpdateStudentDto> updateValidator,
        IPasswordHasher passwordHasher) : IStudentService
    {
        public async Task<PageResult<StudentResponseDto>> GetStudentsPagenation(int page, int pageSize)
        {
            return await studentRepository.GetStudentsPagenation(page, pageSize);
        }
        public async Task<List<CourseResponseDto>> GetStudentCourses(int id)
        {
            var student = await studentRepository.GetStudentById(id) ??
                throw new NotFoundException($"Student with ID {id} not found");            
            return await studentRepository.GetStudentCourses(id);
        }
        public async Task<StudentResponseDto?> GetStudentById(int id)
        {
            var student = await studentRepository.GetStudentById(id) ??
                throw new NotFoundException($"Student with ID {id} not found");
            return student?.ToStudentResponseDto();
        }
        public async Task<TokenizedStudentDto> CreateStudent(CreateStudentDto Dto)
        {
            await createValidator.ValidateAndThrowAsync(Dto);
            var student = Dto.ToStudentEntity();
            student.PasswordHash = passwordHasher.HashPassword(Dto.Password);
            var created = await studentRepository.CreateStudent(student);
            return created;
        }
        public async Task<StudentResponseDto?> UpdateStudent(int id, UpdateStudentDto dto)
        {
            await updateValidator.ValidateAndThrowAsync(dto);

            var student = await studentRepository.GetStudentById(id)??
                throw new NotFoundException($"Student with ID {id} not found");
           
            student.FullName = dto.FullName;
            student.Email = dto.Email;
            var updated = await studentRepository.UpdateStudent(student);
            return updated.ToStudentResponseDto();
        }
        public async Task<Student?> DeleteStudent(int id)
        {
           var student = await studentRepository.GetStudentById(id)??
                throw new NotFoundException($"Student with ID {id} not found");

                await studentRepository.DeleteStudent(student);
            return student;
        }

    }
}
