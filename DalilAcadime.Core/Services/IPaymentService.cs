using DalilAcadime.Core.DTOs;

namespace DalilAcadime.Core.Services;

public interface IPaymentService
{
    Task<PayPaymentResponseDto> PayAsync(int enrollmentId);

    Task<EnrollmentResponseDto> CaptureAsync(int enrollmentId);
}
