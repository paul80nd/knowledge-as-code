#!/bin/sh
# Writes the checkout, whose payment client retries a timed-out authorisation with a new key on every attempt.
set -eu

mkdir -p src/Checkout

cat > src/Checkout/Checkout.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
EOF

cat > src/Checkout/Program.cs <<'EOF'
using Checkout;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PaymentClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentApi"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.MapPost("/orders/{orderId}/pay", async (string orderId, PayRequest request, PaymentClient payments) =>
{
    var outcome = await payments.AuthoriseAsync(orderId, request.Amount, request.Currency, request.CardToken);
    return outcome.Approved ? Results.Ok(outcome) : Results.UnprocessableEntity(outcome);
});

app.Run();

public sealed record PayRequest(decimal Amount, string Currency, string CardToken);
EOF

cat > src/Checkout/PaymentClient.cs <<'EOF'
using System.Net.Http.Json;

namespace Checkout;

public sealed record AuthorisationOutcome(bool Approved, string PspReference, string? DeclineCode);

public sealed class PaymentClient(HttpClient http, ILogger<PaymentClient> logger)
{
    private const int Attempts = 3;

    public async Task<AuthorisationOutcome> AuthoriseAsync(string orderId, decimal amount, string currency, string cardToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/authorisations")
            {
                Content = JsonContent.Create(new { orderId, amount, currency, cardToken }),
            };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

            try
            {
                var response = await http.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return (await response.Content.ReadFromJsonAsync<AuthorisationOutcome>())!;
            }
            catch (TaskCanceledException) when (attempt < Attempts)
            {
                logger.LogWarning("Authorisation attempt {Attempt} for order {OrderId} timed out", attempt, orderId);
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt));
            }
        }
    }
}
EOF

cat > src/Checkout/appsettings.json <<'EOF'
{
  "Services": {
    "PaymentApi": "https://payment-api-dev.example.com"
  }
}
EOF

cat > README.md <<'EOF'
# checkout

The web shop's checkout. It takes the customer's card token for an order and asks the payment API to authorise the
amount.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the checkout"
