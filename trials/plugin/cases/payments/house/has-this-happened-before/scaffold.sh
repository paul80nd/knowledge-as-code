#!/bin/sh
# Writes the payment API, whose authorisation calls the PSP and then the ledger, one after the other.
set -eu

mkdir -p src/PaymentApi

cat > src/PaymentApi/PaymentApi.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
EOF

cat > src/PaymentApi/Program.cs <<'EOF'
using PaymentApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PspClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Psp:BaseUrl"]!));
builder.Services.AddHttpClient<LedgerClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:Ledger"]!));

var app = builder.Build();

app.MapPost("/authorisations", async (AuthorisationRequest request, PspClient psp, LedgerClient ledger) =>
{
    var outcome = await psp.AuthoriseAsync(request);
    await ledger.RecordAsync(request.OrderId, outcome);
    return outcome.Approved ? Results.Ok(outcome) : Results.UnprocessableEntity(outcome);
});

app.Run();
EOF

cat > src/PaymentApi/Clients.cs <<'EOF'
namespace PaymentApi;

public sealed record AuthorisationRequest(string OrderId, decimal Amount, string Currency, string CardToken);

public sealed record AuthorisationOutcome(bool Approved, string PspReference, string? DeclineCode);

public sealed class PspClient(HttpClient http)
{
    public async Task<AuthorisationOutcome> AuthoriseAsync(AuthorisationRequest request)
    {
        var response = await http.PostAsJsonAsync("/v3/authorisations", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthorisationOutcome>())!;
    }
}

public sealed class LedgerClient(HttpClient http)
{
    public async Task RecordAsync(string orderId, AuthorisationOutcome outcome)
    {
        var response = await http.PostAsJsonAsync($"/payments/{orderId}/authorisations", outcome);
        response.EnsureSuccessStatusCode();
    }
}
EOF

cat > src/PaymentApi/appsettings.json <<'EOF'
{
  "Psp": {
    "BaseUrl": "https://api.psp.example.com"
  },
  "Services": {
    "Ledger": "https://payment-ledger-dev.example.com"
  }
}
EOF

cat > README.md <<'EOF'
# payment-api

Takes an authorisation request from the checkout, asks the PSP to authorise it, and records the outcome in the
payment ledger before it replies.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the payment API"
