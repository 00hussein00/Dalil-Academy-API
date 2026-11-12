using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Services
{
    public interface IStudentService
    {
        Task<PageResult<StudentResponseDto>> GetStudentsPagenation(int page, int pageSize);
        Task<List<CourseResponseDto>> GetStudentCourses(int id);
        Task<StudentResponseDto?> GetStudentById(int id);
        Task<TokenizedStudentDto> CreateStudent(CreateStudentDto dto);
        Task<StudentResponseDto?> UpdateStudent(int id, UpdateStudentDto dto);
        Task<Student?> DeleteStudent(int id);


    }
}
