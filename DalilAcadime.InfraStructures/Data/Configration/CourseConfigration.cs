using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class CourseConfigration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c=>c.Id);
            
            builder.Property(c=>c.Title).IsRequired().HasMaxLength(200);
            
            builder.HasIndex(c => c.Title).IsUnique();
            
            builder.Property(c => c.Price).IsRequired();

            builder.Property(c => c.Level)
                    .HasConversion<string>()
                    .IsRequired();

            builder.HasOne(c => c.Instructor)
                   .WithMany(i => i.Courses)
                   .HasForeignKey(c => c.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
