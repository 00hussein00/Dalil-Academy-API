namespace DalilAcadime.Core.DTOs;

public record PayPaymentResponseDto(
    int PaymentId,
    string OrderId,
    decimal Amount,
    string Currency,
    string ApprovalUrl);
