using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; }= string.Empty;
        public string Role { get; set; } = UserRoles.Student;

    }
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Instructor = "Instructor";
        public const string Student = "Student";
    }
}
