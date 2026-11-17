using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Payments;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Services;
using DalilAcadime.InfraStructures.Data;
using Microsoft.EntityFrameworkCore;

namespace DalilAcadime.InfraStructures.Services;

public sealed class PaymentService(
    AppDbContext db,
    IPaymentGateway paymentGateway,
    IEnrollmentRepository enrollmentRepository,
    PayPalOptions options) : IPaymentService
{
    public async Task<PayPaymentResponseDto> PayAsync(int enrollmentId)
    {
        var enrollment = await db.Enrollments
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId)
            ?? throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");

        if (enrollment.Status == Enrollment.EnrollmentStatus.Paid)
        {
            throw new ConflictException("This enrollment is already paid.");
        }

        if (enrollment.Course is null || options.JodToUsdRate <= 0)
        {
            throw new PaymentFailedException("The course price or currency rate is invalid.");
        }

        var amount = Math.Round(
            (decimal)enrollment.Course.Price * options.JodToUsdRate,
            2,
            MidpointRounding.AwayFromZero);

        if (amount <= 0)
        {
            throw new PaymentFailedException("The payment amount must be greater than zero.");
        }

        var order = await paymentGateway.CreateOrderAsync(amount);
        var payment = new Payment
        {
            EnrollmentId = enrollment.Id,
            PayPalOrderId = order.OrderId,
            Amount = amount,
            Currency = "USD",
            Status = Payment.PaymentStatus.Created
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        return new PayPaymentResponseDto(
            payment.Id,
            payment.PayPalOrderId,
            payment.Amount,
            payment.Currency,
            order.ApprovalUrl);
    }

    public async Task<EnrollmentResponseDto> CaptureAsync(int enrollmentId)
    {
        var enrollment = await db.Enrollments
            .FirstOrDefaultAsync(x => x.Id == enrollmentId)
            ?? throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");

        if (enrollment.Status == Enrollment.EnrollmentStatus.Paid)
        {
            throw new ConflictException("This enrollment is already paid.");
        }

        var payment = await db.Payments
            .Where(x => x.EnrollmentId == enrollmentId && x.Status == Payment.PaymentStatus.Created)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new PaymentFailedException("Call /pay first.");

        if (payment.CreatedAt.AddHours(3) <= DateTime.UtcNow)
        {
            payment.Status = Payment.PaymentStatus.Failed;
            await db.SaveChangesAsync();
            throw new PaymentFailedException("The payment expired. Call /pay again.");
        }

        try
        {
            await paymentGateway.CaptureOrderAsync(payment.PayPalOrderId);
        }
        catch (PaymentFailedException)
        {
            payment.Status = Payment.PaymentStatus.Failed;
            await db.SaveChangesAsync();
            throw;
        }
        catch (HttpRequestException)
        {
            payment.Status = Payment.PaymentStatus.Failed;
            await db.SaveChangesAsync();
            throw;
        }

        payment.Status = Payment.PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        enrollment.Status = Enrollment.EnrollmentStatus.Paid;
        await db.SaveChangesAsync();

        return await enrollmentRepository.GetById(enrollmentId)
            ?? throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");
    }
}
