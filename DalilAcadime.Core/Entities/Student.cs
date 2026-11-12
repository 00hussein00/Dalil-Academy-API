using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = UserRoles.Student;
        public int? UserId { get; set; }
        public Users? User { get; set; }
        public List<Enrollment> Enrollments { get; set; } = [];

    }
}
