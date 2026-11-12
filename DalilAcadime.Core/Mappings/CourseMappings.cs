using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;

namespace DalilAcadime.Core.Mappings
{
    public static class CourseMappings
    {
        public static CourseResponseDto ToCourseResponseDto(this Course course)
        {
            return new CourseResponseDto(
                course.Id,
                course.Title,
                course.Price,
                course.Level,
                course.Instructor?.FullName,
                course.ImageUrl
            );
        }
        public static Course ToCourseEntity(this CreateCourseDto dto)
        {
            return new Course
            {
                Title = dto.Title,
                Price = dto.Price,
                Level = dto.Level,
                InstructorId = dto.InstructorId,
                ImageUrl = dto.ImageUrl
            };
        }

    }
}
