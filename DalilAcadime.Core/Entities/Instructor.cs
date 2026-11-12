using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Entities
{
    public class Instructor
    {
        public int Id { get; set; }
        // the EF know its the primary key because of the name Id
        public string FullName { get; set; }
        // the EF know its not null because of the type its not nullable

        public List<Course> Courses { get; set; } = [];

    }
}