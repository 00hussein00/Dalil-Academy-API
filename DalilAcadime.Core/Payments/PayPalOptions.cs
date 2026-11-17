namespace DalilAcadime.Core.Payments;

public sealed class PayPalOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";

    public string ReturnUrl { get; set; } = string.Empty;

    public decimal JodToUsdRate { get; set; } = 1.41m;
}
