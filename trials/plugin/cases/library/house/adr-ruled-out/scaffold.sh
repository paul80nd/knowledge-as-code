#!/bin/sh
# Writes the platform repository's reserve flow, which passes its state from page to page and keeps none.
set -eu

mkdir -p src/Services/Reservations/Pages/Reserve

cat > src/Services/Reservations/Reservations.Web.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
EOF

cat > src/Services/Reservations/Program.cs <<'EOF'
using Reservations.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient<LendingClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Apis:Lending:Url"]!));

var app = builder.Build();

app.MapRazorPages();
app.Run();
EOF

cat > src/Services/Reservations/LendingClient.cs <<'EOF'
namespace Reservations.Web;

public sealed record ProvisionalHold(string Reference);

public sealed class LendingClient(HttpClient http)
{
    public async Task<ProvisionalHold> CreateProvisionalHoldAsync(string cardNumber, string itemId, string branchId)
    {
        var response = await http.PostAsJsonAsync("/holds", new { cardNumber, itemId, branchId, provisional = true });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProvisionalHold>())!;
    }
}
EOF

cat > src/Services/Reservations/Pages/Reserve/Branch.cshtml.cs <<'EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Reservations.Web.Pages.Reserve;

// Step 1: the borrower chooses where to collect the item. The choice travels to the next page in the form.
public sealed class BranchModel : PageModel
{
    [BindProperty(SupportsGet = true)] public string ItemId { get; set; } = "";
    [BindProperty] public string BranchId { get; set; } = "";

    public IActionResult OnPost() => RedirectToPage("Confirm", new { ItemId, BranchId });
}
EOF

cat > src/Services/Reservations/Pages/Reserve/Confirm.cshtml.cs <<'EOF'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Reservations.Web.Pages.Reserve;

// Step 2: the borrower confirms. The provisional hold is the first thing written anywhere.
public sealed class ConfirmModel(LendingClient lending) : PageModel
{
    [BindProperty(SupportsGet = true)] public string ItemId { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string BranchId { get; set; } = "";
    [BindProperty] public string CardNumber { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        var hold = await lending.CreateProvisionalHoldAsync(CardNumber, ItemId, BranchId);
        return RedirectToPage("Pay", new { hold.Reference });
    }
}
EOF

cat > src/Services/Reservations/appsettings.json <<'EOF'
{
  "Apis": {
    "Lending": {
      "Url": "https://app-lending-dev.example.net"
    }
  }
}
EOF

cat > README.md <<'EOF'
# platform

The consortium's web estate. `src/Services/Reservations` is the reserve-and-pay flow, deployed on its own as the
`reservations` app and served at `reserve.example.com`.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the platform repository"
