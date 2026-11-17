using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Users> Users => Set<Users>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSeeding((context, _) =>
            {
                if (!context.Set<Users>().Any())
                {
                    context.Set<Users>().AddRange(
                        new Users
                        {
                            FullName = "Admin User",
                            Email = "admin@a.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                            Role = "Admin"
                        },
                        new Users
                        {
                            FullName = "Instructor User",
                            Email = "instructor@i.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("instructor123"),
                            Role = "Instructor"
                        },
                        new Users
                        {
                            FullName = "Student User",
                            Email = "student@s.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                            Role = "Student"
                        }
                    );
                    context.SaveChanges();
                }

                if (!context.Set<Student>().Any())
                {
                    context.Set<Student>().AddRange(
                        new Student { FullName = "Ahmed", Email = "ahmed@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123") },
                        new Student { FullName = "Sara", Email = "sara@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123") }
                    );
                    context.SaveChanges();
                }

                if (!context.Set<Instructor>().Any())
                {
                    var hussein = new Instructor { FullName = "Hussein" };
                    var maher = new Instructor { FullName = "Maher" };

                    context.Set<Instructor>().AddRange(hussein, maher);
                    context.SaveChanges();

                    context.Set<Course>().AddRange(
                        new Course { Title = "C# Programming", Price = 500f, InstructorId = hussein.Id, Level = Course.CourseLevel.Beginner },
                        new Course { Title = "Python Programming", Price = 50f, Level = Course.CourseLevel.Advanced, InstructorId = maher.Id }
                    );
                    context.SaveChanges();
                }

                if (!context.Set<Enrollment>().Any())
                {
                    var ahmed = context.Set<Student>().First(s => s.Email == "ahmed@example.com");
                    var sara = context.Set<Student>().First(s => s.Email == "sara@example.com");
                    var csharpCourse = context.Set<Course>().First(c => c.Title == "C# Programming");
                    var pythonCourse = context.Set<Course>().First(c => c.Title == "Python Programming");

                    var paidEnrollment = new Enrollment
                    {
                        StudentId = ahmed.Id,
                        CourseId = csharpCourse.Id,
                        EnrollmentAt = DateTime.UtcNow,
                        Status = Enrollment.EnrollmentStatus.Paid
                    };

                    context.Set<Enrollment>().AddRange(
                        paidEnrollment,
                        new Enrollment
                        {
                            StudentId = sara.Id,
                            CourseId = pythonCourse.Id,
                            EnrollmentAt = DateTime.UtcNow,
                            Status = Enrollment.EnrollmentStatus.Pending
                        }
                    );
                    context.SaveChanges();

                    context.Set<Payment>().Add(
                        new Payment
                        {
                            EnrollmentId = paidEnrollment.Id,
                            PayPalOrderId = "DEV-PAYPAL-ORDER-001",
                            Amount = (decimal)csharpCourse.Price,
                            Status = Payment.PaymentStatus.Completed,
                            CreatedAt = DateTime.UtcNow,
                            PaidAt = DateTime.UtcNow
                        }
                    );
                    context.SaveChanges();
                }
            });
        }
    }
}