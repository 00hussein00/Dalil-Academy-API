using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class InstructorConfigrationcs : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(i => i.Courses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
