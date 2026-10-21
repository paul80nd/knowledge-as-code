#!/bin/sh
# Writes the payment ledger, whose daily reconciliation cuts the day at UK local midnight.
set -eu

mkdir -p src/PaymentLedger/Reconciliation

cat > src/PaymentLedger/PaymentLedger.csproj <<'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
EOF

cat > src/PaymentLedger/Reconciliation/ReconciliationWindow.cs <<'EOF'
namespace PaymentLedger.Reconciliation;

// The span of ledger entries one day's run compares with that day's settlement file.
public static class ReconciliationWindow
{
    private static readonly TimeZoneInfo London = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    public static (DateTimeOffset Start, DateTimeOffset End) For(DateOnly day)
    {
        var midnight = day.ToDateTime(TimeOnly.MinValue);
        var nextMidnight = day.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return (new DateTimeOffset(midnight, London.GetUtcOffset(midnight)),
                new DateTimeOffset(nextMidnight, London.GetUtcOffset(nextMidnight)));
    }
}
EOF

cat > src/PaymentLedger/Reconciliation/DailyRun.cs <<'EOF'
namespace PaymentLedger.Reconciliation;

public sealed record LedgerEntry(string PspReference, string Kind, long AmountMinor, string Currency, DateTimeOffset WrittenAt);

public sealed record SettlementRow(string PspReference, long AmountMinor, string Currency);

public sealed record Break(string PspReference, string Reason, long AmountMinor);

public interface ILedgerStore
{
    Task<IReadOnlyList<LedgerEntry>> EntriesBetweenAsync(DateTimeOffset start, DateTimeOffset end);
}

public interface ISettlementFiles
{
    Task<IReadOnlyList<SettlementRow>?> ForDayAsync(DateOnly day);
}

public sealed class DailyRun(ILedgerStore ledger, ISettlementFiles files)
{
    public async Task<IReadOnlyList<Break>> RunAsync(DateOnly day)
    {
        var rows = await files.ForDayAsync(day)
            ?? throw new InvalidOperationException($"No settlement file for {day:yyyy-MM-dd}");
        var (start, end) = ReconciliationWindow.For(day);
        var entries = await ledger.EntriesBetweenAsync(start, end);

        var byReference = entries.ToDictionary(e => e.PspReference);
        var breaks = new List<Break>();
        foreach (var row in rows)
        {
            if (!byReference.Remove(row.PspReference, out var entry))
                breaks.Add(new Break(row.PspReference, "in file, not in ledger", row.AmountMinor));
            else if (entry.AmountMinor != row.AmountMinor || entry.Currency != row.Currency)
                breaks.Add(new Break(row.PspReference, "amounts differ", row.AmountMinor));
        }
        breaks.AddRange(byReference.Values.Select(e => new Break(e.PspReference, "in ledger, not in file", e.AmountMinor)));
        return breaks;
    }
}
EOF

cat > README.md <<'EOF'
# payment-ledger

Records every event on a payment: the authorisation, the capture and the refund. Every night it reconciles the
ledger against the PSP's settlement file for the previous day, which arrives by SFTP at 02:00.
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the payment ledger"
