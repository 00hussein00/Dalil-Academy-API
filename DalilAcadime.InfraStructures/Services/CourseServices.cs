using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Mappings;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Services;
using FluentValidation;

namespace DalilAcadime.InfraStructures.Services
{
    public class CourseServices(
        ICourseRepository courseRepository,
        IValidator<CreateCourseDto> createValidator,
        IValidator<UpdateCourseDto> updateValidator) : ICourseServices
    {
        public  Task<PageResult<CourseResponseDto>> GetCoursesPagenation(int page, int pageSize, double minPrice, double maxPrice)
        {
            return courseRepository.GetCoursesPagenation(page, pageSize, minPrice, maxPrice);
        }
        public Task<int> CountCourses()
        {
            return courseRepository.CountCourses();
        }

        public async Task<CourseResponseDto?> GetCourseById(int id) {

            var course = await courseRepository.GetCourseById(id) ??
                throw new NotFoundException($"Course with ID {id} not found");
            return course?.ToCourseResponseDto();
        }
        public  async Task<CourseResponseDto> CreateCourse(CreateCourseDto dto) {
           
            await createValidator.ValidateAndThrowAsync(dto);
            var course = dto.ToCourseEntity();

            var created = await courseRepository.CreateCourse(course);
            return  created.ToCourseResponseDto();
        }

        public async Task<CourseResponseDto?> UpdateCourse(int id, UpdateCourseDto dto) {
            await updateValidator.ValidateAndThrowAsync(dto);
            
            var course = await courseRepository.GetCourseById(id)
                    ?? throw new NotFoundException($"Course with ID {id} not found");

            course.Title = dto.Title;
            course.Price = dto.Price;
            course.Level = dto.Level;
            course.InstructorId = dto.InstructorId;
            course.ImageUrl = dto.ImageUrl;

            var updated = await courseRepository.UpdateCourse(course);

            return updated?.ToCourseResponseDto();
        }

        public async Task<Course?> DeleteCourse(int id)
        {
            var course = await courseRepository.GetCourseById(id) ??
                throw new NotFoundException($"Course with ID {id} not found");
            
            await courseRepository.DeleteCourse(course);
            return course;
        }
    }
}
