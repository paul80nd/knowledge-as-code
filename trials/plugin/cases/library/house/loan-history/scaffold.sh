#!/bin/sh
# Writes the lending API over the legacy CIRC loan table, with one route for a borrower's open loans.
set -eu

mkdir -p src/Lending.Api

cat > src/Lending.Api/Lending.Api.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
  </ItemGroup>

</Project>
EOF

cat > src/Lending.Api/Program.cs <<'EOF'
using Lending.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LegacyDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Legacy")));

var app = builder.Build();

app.MapGet("/borrowers/{card}/loans", async (string card, LegacyDb db) =>
    await db.Loans
        .Where(loan => loan.CardNumber == card && loan.DateBack == null)
        .OrderBy(loan => loan.DateDue)
        .Select(loan => new LoanView(loan.ItemBarcode, loan.BranchCode, loan.DateOut, loan.DateDue, loan.DateBack))
        .ToListAsync());

app.Run();
EOF

cat > src/Lending.Api/LegacyDb.cs <<'EOF'
using Microsoft.EntityFrameworkCore;

namespace Lending.Api;

// Maps the legacy system's own tables. The legacy system still writes to them, so nothing here migrates or alters one.
public sealed class LegacyDb(DbContextOptions<LegacyDb> options) : DbContext(options)
{
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Loan>(loan =>
        {
            loan.ToTable("CIRC_LOAN");
            loan.HasKey(l => l.LoanId);
            loan.Property(l => l.LoanId).HasColumnName("LOAN_ID");
            loan.Property(l => l.ItemBarcode).HasColumnName("ITEM_BARCODE");
            loan.Property(l => l.CardNumber).HasColumnName("CARD_NO");
            loan.Property(l => l.BranchCode).HasColumnName("BRANCH_CD");
            loan.Property(l => l.DateOut).HasColumnName("DATE_OUT");
            loan.Property(l => l.DateDue).HasColumnName("DATE_DUE");
            loan.Property(l => l.DateBack).HasColumnName("DATE_BACK");
        });
    }
}
EOF

cat > src/Lending.Api/Loan.cs <<'EOF'
namespace Lending.Api;

// One row of CIRC_LOAN: one item issued to one borrower. DateBack is null while the item is out.
public sealed class Loan
{
    public int LoanId { get; set; }
    public string ItemBarcode { get; set; } = "";
    public string? CardNumber { get; set; }
    public string BranchCode { get; set; } = "";
    public DateTime DateOut { get; set; }
    public DateTime DateDue { get; set; }
    public DateTime? DateBack { get; set; }
}

public sealed record LoanView(string ItemBarcode, string BranchCode, DateTime DateOut, DateTime DateDue, DateTime? DateBack);
EOF

cat > src/Lending.Api/appsettings.json <<'EOF'
{
  "ConnectionStrings": {
    "Legacy": ""
  }
}
EOF

cat > README.md <<'EOF'
# lending

Loans, returns and holds, over the legacy library management system. `src/Lending.Api` maps onto the legacy `CIRC`
tables and owns no schema of its own: the legacy system still writes to the same database. It deploys as the
`app-lending-<env>` App Service.
EOF

cat > .gitignore <<'EOF'
bin/
obj/
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the lending API"
