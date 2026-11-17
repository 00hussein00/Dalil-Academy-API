namespace DalilAcadime.Core.Payments;

public interface IPaymentGateway
{
    Task<(string OrderId, string ApprovalUrl)> CreateOrderAsync(decimal amount);

    Task<bool> CaptureOrderAsync(string orderId);
}
