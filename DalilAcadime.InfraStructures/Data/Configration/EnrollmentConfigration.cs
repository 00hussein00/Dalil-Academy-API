using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class EnrollmentConfigration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            
            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Enrollments)
                   .HasForeignKey(e => e.CourseId);

            builder.HasOne(e => e.Student)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.StudentId);

            builder.Property(e => e.EnrollmentAt)
                   .HasDefaultValueSql("now()");

            builder.Property(e => e.Status)
                   .HasConversion<string>();

        }
    }
}
