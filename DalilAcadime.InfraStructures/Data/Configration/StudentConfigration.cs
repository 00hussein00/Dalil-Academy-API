using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class StudentConfigration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired();

            builder.HasIndex(c => c.Email)
                .IsUnique();

            builder.Property(c => c.PasswordHash)
                .IsRequired()
                .HasMaxLength(250);



        }
    }
}
