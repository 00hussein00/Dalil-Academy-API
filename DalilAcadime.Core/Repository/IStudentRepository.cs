using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Repository
{
    public interface IStudentRepository
    {
        Task<PageResult<StudentResponseDto>> GetStudentsPagenation(int page, int pageSize);
        Task<List<CourseResponseDto>> GetStudentCourses(int id);
        Task<Student?> GetStudentById(int id);
        Task<TokenizedStudentDto> CreateStudent(Student student);
        Task<Student?> UpdateStudent(Student updatedStudent);
        Task DeleteStudent(Student student);
    }
}
