using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public DateTime? EnrollmentAt { get; set; } 
    }
}
