#!/bin/sh
# Writes a .NET order repository over Postgres with a unit test project and no integration tests.
set -eu

mkdir -p src/Orders tests/Orders.Tests db

cat > src/Orders/Orders.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql" Version="8.0.5" />
  </ItemGroup>
</Project>
EOF

cat > src/Orders/OrderRepository.cs <<'EOF'
using System.Globalization;
using Npgsql;

namespace Orders;

public sealed record Order(long Id, string FullName, int TotalPence);

public sealed class OrderRepository(NpgsqlDataSource db)
{
    public async Task<long> AddAsync(string fullName, int totalPence)
    {
        await using var cmd = db.CreateCommand(
            "INSERT INTO orders (full_name, total_pence) VALUES ($1, $2) RETURNING id");
        cmd.Parameters.AddWithValue(fullName);
        cmd.Parameters.AddWithValue(totalPence);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public async Task<Order?> GetAsync(long id)
    {
        await using var cmd = db.CreateCommand("SELECT id, full_name, total_pence FROM orders WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? new Order(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2))
            : null;
    }
}
EOF

cat > db/schema.sql <<'EOF'
CREATE TABLE orders (
    id          bigserial PRIMARY KEY,
    full_name   text    NOT NULL,
    total_pence integer NOT NULL
);
EOF

cat > tests/Orders.Tests/Orders.Tests.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Orders/Orders.csproj" />
  </ItemGroup>
</Project>
EOF

cat > tests/Orders.Tests/OrderTests.cs <<'EOF'
using Orders;

namespace Orders.Tests;

public class OrderTests
{
    [Fact]
    public void Order_KeepsItsTotal()
    {
        var order = new Order(1, "Alex Fenwick", 2400);
        Assert.Equal(2400, order.TotalPence);
    }
}
EOF

cat > README.md <<'EOF'
# orders

Stores orders in Postgres. The schema is `db/schema.sql`.

    dotnet test tests/Orders.Tests
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the orders repository"
