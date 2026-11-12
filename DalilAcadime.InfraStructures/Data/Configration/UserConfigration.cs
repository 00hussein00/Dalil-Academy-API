using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class UserConfigration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(250);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(250);
            ;
            builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        }
    
    }
}
