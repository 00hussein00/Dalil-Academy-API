using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Entities
{
    public class Enrollment
    {
        public enum EnrollmentStatus
        {
            Pending,
            Paid
        }
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public DateTime? EnrollmentAt { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        //public List<Payment> PaymentHistory { get; set; } = [];

    }
}
