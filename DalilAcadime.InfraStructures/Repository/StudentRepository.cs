using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Security;
using DalilAcadime.InfraStructures.Data;
using DalilAcadime.InfraStructures.Security;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace DalilAcadime.InfraStructures.Repository
{
    public class StudentRepository(AppDbContext db, ITokenService tokenService) : IStudentRepository
    {
        public async Task<PageResult<StudentResponseDto>> GetStudentsPagenation(int page, int pageSize)
        {
            var totalCount = await db.Students.CountAsync();
            var items = await db.Students
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentResponseDto(s.Id, s.FullName, s.Email))
                .ToListAsync();
            return new PageResult<StudentResponseDto>(items,page,pageSize,totalCount);
        }

        public async Task<List<CourseResponseDto>> GetStudentCourses(int studentId)
        {
            var courses = await db.Courses
                .Where(c => c.Enrollments.Any(s => s.StudentId == studentId))
                .Select(c => new CourseResponseDto(c.Id, c.Title,c.Price, c.Level, c.Instructor!.FullName, c.ImageUrl))
                .ToListAsync();

            return courses;
        }

        public async Task<Student?> GetStudentById(int id)
        {
            return await db.Students.FindAsync(id);
        }

        public async Task<TokenizedStudentDto> CreateStudent(Student student)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var user = new Users
                {
                    FullName = student.FullName,
                    Email = student.Email,
                    PasswordHash = student.PasswordHash,
                    Role = UserRoles.Student
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                student.UserId = user.Id;
                student.Role = UserRoles.Student;
                
                db.Students.Add(student);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                var (token, expiration) = tokenService.GenerateToken(student);
                return new TokenizedStudentDto(student.Id, student.FullName, student.Email, token, expiration);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                await transaction.RollbackAsync();
                throw new ConflictException("Email must be unique.");
            }
        }
        public async Task<Student?> UpdateStudent(Student updatedStudent)
        {
            try
            {
                await db.SaveChangesAsync();
                return updatedStudent;
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("Email must be unique.");
            }
        }
        public async Task DeleteStudent(Student student)
        {
            db.Students.Remove(student);
            await db.SaveChangesAsync();
        }   
    }
}
