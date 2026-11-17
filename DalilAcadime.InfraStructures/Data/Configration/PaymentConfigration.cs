using DalilAcadime.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DalilAcadime.InfraStructures.Data.Configration
{
    public class PaymentConfigration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasIndex(p => p.Id);
            builder.HasIndex(p => p.PayPalOrderId)
                .IsUnique();

            builder.Property(p => p.PayPalOrderId)
                .IsRequired();

            builder.Property(p => p.Amount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");


            builder.HasOne(p => p.Enrollment)
                .WithMany()
                .HasForeignKey(p => p.EnrollmentId)
                .IsRequired();

        }
    }
}
