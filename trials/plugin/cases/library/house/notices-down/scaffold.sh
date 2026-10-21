#!/bin/sh
# Writes the notices function app, whose overdue sweep and hold-ready function send email through the mail provider.
set -eu

mkdir -p src/Notices.FunctionApp

cat > src/Notices.FunctionApp/Notices.FunctionApp.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.ServiceBus" Version="5.22.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Timer" Version="4.3.1" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.0" />
  </ItemGroup>

</Project>
EOF

cat > src/Notices.FunctionApp/Program.cs <<'EOF'
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notices.FunctionApp;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => services.AddHttpClient<MailClient>())
    .Build();

host.Run();
EOF

cat > src/Notices.FunctionApp/HoldReady.cs <<'EOF'
using Microsoft.Azure.Functions.Worker;

namespace Notices.FunctionApp;

public sealed record HoldReadyEvent(string HoldId, string RecipientEmail, string Title, string BranchName);

public sealed class HoldReady(MailClient mail)
{
    [Function(nameof(HoldReady))]
    public Task Run([ServiceBusTrigger("notices-hold-ready", Connection = "ServiceBus")] HoldReadyEvent hold)
        => mail.SendAsync("hold-ready", hold.RecipientEmail, new { hold.Title, hold.BranchName });
}
EOF

cat > src/Notices.FunctionApp/MailClient.cs <<'EOF'
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Notices.FunctionApp;

// The mail provider stores the wording. This sends the template name and the values that fill it.
public sealed class MailClient(HttpClient http, IConfiguration config)
{
    public async Task SendAsync(string template, string to, object values)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, config["Mail:BaseUrl"])
        {
            Content = JsonContent.Create(new { template, to, values }),
        };
        request.Headers.Authorization = new("Bearer", config["Mail:Token"]);
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
EOF

cat > src/Notices.FunctionApp/local.settings.json <<'EOF'
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBus": "",
    "Mail__BaseUrl": "https://api.hartwell.example.com/v2/send",
    "Mail__Token": ""
  }
}
EOF

cat > src/Notices.FunctionApp/OverdueSweep.cs <<'EOF'
using Microsoft.Azure.Functions.Worker;

namespace Notices.FunctionApp;

public sealed record OverdueLoan(string LoanId, string RecipientEmail, string Title, DateOnly DueOn);

public interface IOverdueLoans
{
    Task<IReadOnlyList<OverdueLoan>> UnnoticedAsync();
}

public sealed class OverdueSweep(IOverdueLoans loans, MailClient mail)
{
    [Function(nameof(OverdueSweep))]
    public async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo timer)
    {
        foreach (var loan in await loans.UnnoticedAsync())
            await mail.SendAsync("overdue", loan.RecipientEmail, new { loan.Title, loan.DueOn });
    }
}
EOF

cat > README.md <<'EOF'
# notices

The emails the library sends: overdue reminders, hold-ready alerts and membership renewals.
`src/Notices.FunctionApp` deploys as the `func-notices-<env>` Function App.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the notices function app"
