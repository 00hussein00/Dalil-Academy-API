using DalilAcadime.Core.Payments;
using DalilAcadime.Core.Exceptions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace DalilAcadime.InfraStructures.Payments;

public sealed class PayPalPaymentGateway(HttpClient httpClient, PayPalOptions options) : IPaymentGateway
{
    // Creates a PayPal order and returns the URL where the customer approves the payment.
    public async Task<(string OrderId, string ApprovalUrl)> CreateOrderAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new PaymentFailedException("Payment amount must be greater than zero.");
        }

        // PayPal requires an access token for protected API requests.
        using var request = new HttpRequestMessage(HttpMethod.Post, "v2/checkout/orders")
        {
            Content = JsonContent.Create(new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = "USD",
                            value = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    return_url = options.ReturnUrl,
                    cancel_url = options.ReturnUrl
                }
            })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());

        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response, "creating the PayPal order");

        // The approval link is returned in the order links collection.
        var order = await response.Content.ReadFromJsonAsync<PayPalOrder>()
            ?? throw new PaymentFailedException("PayPal returned an empty order.");
        var approvalUrl = order.Links.FirstOrDefault(x => x.Rel == "approve")?.Href
            ?? throw new PaymentFailedException("PayPal approval URL was not returned.");

        return (order.Id, approvalUrl);
    }

    // Captures an approved order and returns true only when PayPal reports COMPLETED.
    public async Task<bool> CaptureOrderAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new PaymentFailedException("Order ID is required.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture")
        {
            Content = JsonContent.Create(new { })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());

        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response, "capturing the PayPal order");

        var result = await response.Content.ReadFromJsonAsync<PayPalCapture>();
        if (!string.Equals(result?.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentFailedException("PayPal did not complete the payment.");
        }

        return true;
    }

    // Gets an OAuth access token using the PayPal client credentials.
    private async Task<string> GetTokenAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            })
        };

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{options.ClientId}:{options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response, "getting the PayPal access token");

        var token = await response.Content.ReadFromJsonAsync<PayPalToken>()
            ?? throw new PaymentFailedException("PayPal returned an empty token.");

        return token.AccessToken;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync();
        throw new PaymentFailedException($"PayPal failed while {operation}: {details}");
    }

    private sealed class PayPalToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class PayPalOrder
    {
        public string Id { get; set; } = string.Empty;
        public List<PayPalLink> Links { get; set; } = [];
    }

    private sealed class PayPalLink
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
    }

    private sealed class PayPalCapture
    {
        public string Status { get; set; } = string.Empty;
    }
}
