using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Repository
{
    public interface ICourseRepository
    {
        Task<PageResult<CourseResponseDto>> GetCoursesPagenation(int page, int pageSize,double minPrice, double maxPrice);
        Task<int> CountCourses();

        Task<Course?> GetCourseById(int id);
        Task<Course> CreateCourse(Course course);

        Task<Course?> UpdateCourse(Course updatedCourse);
        Task DeleteCourse(Course course);

    }
}
