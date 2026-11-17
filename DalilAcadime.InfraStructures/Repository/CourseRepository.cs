using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Repository;
using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace DalilAcadime.InfraStructures.Repository

{
    public class CourseRepository(AppDbContext db) : ICourseRepository
    {
        public async Task<int> CountCourses()
        {
            return await db.Courses.CountAsync();
        }
        public async Task<PageResult<CourseResponseDto>> GetCoursesPagenation(int page,int pageSize, double minPrice, double maxPrice)
        {
            var count = await db.Courses.CountAsync();
            var items = await db.Courses
                .Where(c => c.Price >= minPrice && c.Price <= maxPrice)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseResponseDto(c.Id, c.Title,c.Price,c.Level, c.Instructor!.FullName, c.ImageUrl))
                .ToListAsync();
            return new PageResult<CourseResponseDto>(items, page, pageSize, count);

        }
        public async Task<Course?> GetCourseById(int id)
        {
            return await db.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<StudentResponseDto>> GetPaidStudents(int courseId)
        {
            if (!await db.Courses.AnyAsync(course => course.Id == courseId))
            {
                throw new NotFoundException($"Course with ID {courseId} not found");
            }

            return await db.Enrollments
                .Where(enrollment =>
                    enrollment.CourseId == courseId &&
                    enrollment.Status == Enrollment.EnrollmentStatus.Paid)
                .Select(enrollment => new StudentResponseDto(
                    enrollment.Student!.Id,
                    enrollment.Student.FullName,
                    enrollment.Student.Email))
                .ToListAsync();
        }

        public async Task<Course> CreateCourse(Course course)
        {
            try
            {
                db.Courses.Add(course);
                await db.SaveChangesAsync();
                await db.Entry(course).Reference(c => c.Instructor).LoadAsync();
                return course;
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("Course Title must be unique.");
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                throw new NotFoundException($"Instructor with ID {course.InstructorId} not found");
            }
        }

        public async Task<Course?> UpdateCourse(Course updateCourse)
        {
            try
            {
                await db.SaveChangesAsync();
                return updateCourse;
            }
            
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("Course Title must be unique.");
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                throw new NotFoundException($"Instructor with ID {updateCourse.InstructorId} not found");
            }

        }

        public async Task DeleteCourse(Course course)
        {
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
        }

    }
}

 