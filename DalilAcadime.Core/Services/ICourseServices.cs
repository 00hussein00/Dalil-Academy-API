using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Services
{
    public interface ICourseServices
    {
        Task<PageResult<CourseResponseDto>> GetCoursesPagenation(int page, int pageSize,double minPrice, double maxPrice);
        Task<int> CountCourses();
        Task<CourseResponseDto?> GetCourseById(int id);
        Task<CourseResponseDto> CreateCourse(CreateCourseDto dto);

        Task<CourseResponseDto?> UpdateCourse(int id, UpdateCourseDto updatedCourse);
        Task<Course?> DeleteCourse(int id);



    }
}
