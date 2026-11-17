using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Repository;
using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalilAcadime.InfraStructures.Repository
{
    public class EnrollmentRepository(AppDbContext db) : IEnrollmentRepository
    {
        public async Task<List<CourseResponseDto>> GetStudentCoursesByUserId(int userId)
        {
            return await db.Courses
                .Where(course => course.Enrollments.Any(enrollment =>
                    enrollment.Student!.UserId == userId &&
                    enrollment.Status == Enrollment.EnrollmentStatus.Paid))
                .Select(course => new CourseResponseDto(
                    course.Id,
                    course.Title,
                    course.Price,
                    course.Level,
                    course.Instructor!.FullName,
                    course.ImageUrl
                ))
                .ToListAsync();
        }

        public async Task<List<EnrollmentResponseDto>> GetAll()
        {
            return await db.Enrollments
                .OrderBy(enrollment => enrollment.Id)
                .Select(enrollment => new EnrollmentResponseDto(
                    enrollment.Id,
                    enrollment.StudentId,
                    enrollment.CourseId,
                    enrollment.Status,
                    enrollment.EnrollmentAt))
                .ToListAsync();
        }

        public async Task<EnrollmentResponseDto?> GetById(int id)
        {
            return await db.Enrollments
                .Where(enrollment => enrollment.Id == id)
                .Select(enrollment => new EnrollmentResponseDto(
                    enrollment.Id,
                    enrollment.StudentId,
                    enrollment.CourseId,
                    enrollment.Status,
                    enrollment.EnrollmentAt))
                .FirstOrDefaultAsync();
        }

        public async Task<EnrollmentResponseDto> Create(Enrollment enrollment)
        {
            var course = await EnsureReferencesExist(enrollment.StudentId, enrollment.CourseId);
            await EnsureEnrollmentDoesNotExist(enrollment.StudentId, enrollment.CourseId);

            if (course.Price == 0f)
            {
                enrollment.Status = Enrollment.EnrollmentStatus.Paid;
            }

            try
            {
                db.Enrollments.Add(enrollment);
                await db.SaveChangesAsync();
                return ToResponse(enrollment);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("The student is already enrolled in this course.");
            }
        }

        public async Task<EnrollmentResponseDto?> Update(int id, int studentId, int courseId)
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            if (enrollment is null)
            {
                return null;
            }

            if (enrollment.Status == Enrollment.EnrollmentStatus.Paid && enrollment.CourseId != courseId)
            {
                throw new ConflictException("A paid enrollment cannot be moved to another course.");
            }

            await EnsureReferencesExist(studentId, courseId);

            var duplicateExists = await db.Enrollments.AnyAsync(existing =>
                existing.Id != id &&
                existing.StudentId == studentId &&
                existing.CourseId == courseId);

            if (duplicateExists)
            {
                throw new ConflictException("The student is already enrolled in this course.");
            }

            enrollment.StudentId = studentId;
            enrollment.CourseId = courseId;

            try
            {
                await db.SaveChangesAsync();
                return ToResponse(enrollment);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("The student is already enrolled in this course.");
            }
        }

        public async Task<EnrollmentResponseDto?> Delete(int id)
        {
            var enrollment = await db.Enrollments.FindAsync(id);
            if (enrollment is null)
            {
                return null;
            }

            if (enrollment.Status == Enrollment.EnrollmentStatus.Paid)
            {
                throw new ConflictException("A paid enrollment cannot be deleted.");
            }

            var response = ToResponse(enrollment);
            db.Enrollments.Remove(enrollment);
            await db.SaveChangesAsync();
            return response;
        }

        private async Task<Course> EnsureReferencesExist(int studentId, int courseId)
        {
            if (!await db.Students.AnyAsync(student => student.Id == studentId))
            {
                throw new NotFoundException($"Student with ID {studentId} not found.");
            }

            var course = await db.Courses.FirstOrDefaultAsync(course => course.Id == courseId);
            if (course is null)
            {
                throw new NotFoundException($"Course with ID {courseId} not found.");
            }

            return course;
        }

        private async Task EnsureEnrollmentDoesNotExist(int studentId, int courseId)
        {
            if (await db.Enrollments.AnyAsync(enrollment =>
                    enrollment.StudentId == studentId && enrollment.CourseId == courseId))
            {
                throw new ConflictException("The student is already enrolled in this course.");
            }
        }

        private static EnrollmentResponseDto ToResponse(Enrollment enrollment)
        {
            return new EnrollmentResponseDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.CourseId,
                enrollment.Status,
                enrollment.EnrollmentAt);
        }

    }
}
