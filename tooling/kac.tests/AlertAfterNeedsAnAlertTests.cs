using kac.core;

// A delay is judged against records the target does not carry, and often two hops away: the NFR names an
// offering, the offering names the services, and only the last of those says whether anything fires. The
// fixture exercises the one-hop case; the rest are the shapes a second fixture corpus would only repeat.

namespace kac.tests;

public class AlertAfterNeedsAnAlertTests
{
    [Fact]
    public void A_target_stating_no_delay_is_left_alone()
        => Assert.Empty(Bound(delay: null, services: [("svc-ledger", "none")]));

    [Fact]
    public void A_delay_on_a_service_that_alerts_is_left_alone()
        => Assert.Empty(Bound("15 minutes", services: [("svc-api", "alert")]));

    // A ticket is filed by the system, so a delay in front of it is the same claim an alert makes.
    [Fact]
    public void A_ticket_raises_something_and_satisfies_the_rule()
        => Assert.Empty(Bound("2 hours", services: [("svc-search", "ticket")]));

    [Fact]
    public void A_delay_on_a_service_that_emits_nothing_is_reported()
    {
        var found = Assert.Single(Bound("15 minutes", services: [("svc-ledger", "none")]));

        Assert.Equal("alert-after-needs-an-alert", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal(
            "'alert-after' states a delay and nothing this binds raises an alert or a ticket: "
            + "'monitoring-output' is 'none'. Bind a service that raises one, or drop 'alert-after'.",
            found.Message);
    }

    // A log is recorded and nothing reads it, so no delay in front of it means anything.
    [Fact]
    public void A_log_raises_nothing_and_is_reported()
        => Assert.Contains("'monitoring-output' is 'log'",
            Assert.Single(Bound("15 minutes", services: [("svc-audit", "log")])).Message);

    // A customer-facing target binds the offering, so the walk has to reach the services under it or the
    // rule never fires on the records that carry a delay.
    [Fact]
    public void The_walk_reaches_the_services_an_offering_implements()
        => Assert.Single(Through("ofr-card-payment", "15 minutes",
            services: [("svc-ledger", "none")]));

    [Fact]
    public void An_offering_with_one_alerting_service_is_left_alone()
        => Assert.Empty(Through("ofr-card-payment", "15 minutes",
            services: [("svc-ledger", "none"), ("svc-api", "alert")]));

    // One service raising something is enough, whatever the rest of them emit.
    [Fact]
    public void One_service_that_alerts_answers_for_all_of_them()
        => Assert.Empty(Bound("15 minutes", services: [("svc-audit", "log"), ("svc-api", "alert")]));

    // `monitoring-output` is required of a live service alone, so a service that states none has not said
    // it emits nothing. Reading silence as a fault would warn on every target bound to a service in build.
    [Fact]
    public void A_service_stating_no_output_suppresses_the_warning()
        => Assert.Empty(Bound("15 minutes", services: [("svc-building", null)]));

    [Fact]
    public void One_service_stating_no_output_suppresses_it_for_the_whole_target()
        => Assert.Empty(Bound("15 minutes",
            services: [("svc-ledger", "none"), ("svc-building", null)]));

    // A dangling id is `ref-resolves`'s to report, and a target reaching nothing has nothing to judge.
    [Fact]
    public void An_id_naming_no_record_is_not_this_rules_business()
        => Assert.Empty(Bound("15 minutes", services: [], appliesTo: ["svc-missing"]));

    // The values are what the author acts on, so the message reads the same however the services were
    // listed and says each one once.
    [Fact]
    public void The_message_states_each_output_once_and_in_order()
        => Assert.Contains("'monitoring-output' is 'log' and 'none'",
            Assert.Single(Bound("15 minutes",
                services: [("svc-c", "none"), ("svc-b", "log"), ("svc-a", "none")])).Message);

    // The targets are read off whichever field references services, so a type declaring none has nothing
    // to walk and the rule stops rather than reaching for a field by name.
    [Fact]
    public void A_type_with_no_field_referencing_services_is_left_alone()
        => Assert.Empty(Run(NfrType(appliesTo: null), "15 minutes", ["svc-ledger"], null,
            [("svc-ledger", "none")]));

    // Two fields of service ids would make the walk follow whichever was declared first. The rule stops
    // instead, because picking one of them is a choice nothing in the schema states.
    [Fact]
    public void A_type_declaring_two_fields_of_service_ids_is_left_alone()
    {
        var second = new FieldSpec
            { Name = "also-binds", Type = "list", Of = "id", Refs = ["services"] };
        var nfrs = new TypeSchema
        {
            Key = "nfrs",
            Folder = "nfrs",
            FieldOrder = [AppliesTo.Name, second.Name, AlertAfter.Name],
            Fields = new[] { AppliesTo, second, AlertAfter }.ToDictionary(f => f.Name)
        };

        Assert.Empty(Run(nfrs, "15 minutes", ["svc-ledger"], null, [("svc-ledger", "none")]));
    }

    private static readonly FieldSpec AppliesTo = new()
        { Name = "applies-to", Type = "list", Of = "id", Refs = ["services", "offerings"] };

    private static readonly FieldSpec ImplementedBy = new()
        { Name = "implemented-by", Type = "list", Of = "id", Refs = ["services"] };

    private static readonly FieldSpec AlertAfter = new() { Name = "alert-after", Type = "string" };

    private static readonly FieldSpec MonitoringOutput = new()
        { Name = "monitoring-output", Type = "enum" };

    private static TypeSchema NfrType(FieldSpec? appliesTo)
    {
        var fields = appliesTo is null ? new[] { AlertAfter } : [appliesTo, AlertAfter];
        return new TypeSchema
        {
            Key = "nfrs",
            Folder = "nfrs",
            FieldOrder = [.. fields.Select(f => f.Name)],
            Fields = fields.ToDictionary(f => f.Name)
        };
    }

    // One target binding the services directly, which is what a property of a deployment does.
    private static List<Finding> Bound(
        string? delay, (string Id, string? Output)[] services, string[]? appliesTo = null)
        => Run(NfrType(AppliesTo), delay, appliesTo ?? [.. services.Select(s => s.Id)], null, services);

    // One target binding an offering, which is what a wait a customer feels does.
    private static List<Finding> Through(
        string offering, string? delay, (string Id, string? Output)[] services)
        => Run(NfrType(AppliesTo), delay, [offering], offering, services);

    private static List<Finding> Run(TypeSchema nfrs, string? delay, string[] appliesTo,
        string? offering, (string Id, string? Output)[] services)
    {
        var offeringType = new TypeSchema
        {
            Key = "offerings",
            Folder = "offerings",
            FieldOrder = [ImplementedBy.Name],
            Fields = new Dictionary<string, FieldSpec> { [ImplementedBy.Name] = ImplementedBy }
        };

        var serviceType = new TypeSchema
        {
            Key = "services",
            Folder = "services",
            FieldOrder = [MonitoringOutput.Name],
            Fields = new Dictionary<string, FieldSpec> { [MonitoringOutput.Name] = MonitoringOutput }
        };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["nfrs"] = nfrs, ["offerings"] = offeringType, ["services"] = serviceType
            }
        };

        var binds = nfrs.FieldOrder.Contains(AppliesTo.Name)
            ? $"{AppliesTo.Name}: [{string.Join(", ", appliesTo)}]\n"
            : "";
        var after = delay is null ? "" : $"{AlertAfter.Name}: {delay}\n";

        var docs = new List<Doc>();
        if (Doc.Parse("nfrs/0001-authorisation.md",
                $"---\nid: nfr-0001\n{binds}{after}---\n\n# A target\n", schema) is { } target)
            docs.Add(target);

        if (offering is not null
            && Doc.Parse($"offerings/{offering}.md",
                $"---\nid: {offering}\n{ImplementedBy.Name}: "
                + $"[{string.Join(", ", services.Select(s => s.Id))}]\n---\n\n# {offering}\n", schema)
            is { } hub)
            docs.Add(hub);

        foreach (var (id, output) in services)
            if (Doc.Parse($"services/{id}.md",
                    $"---\nid: {id}\n{(output is null ? "" : $"{MonitoringOutput.Name}: {output}\n")}"
                    + $"---\n\n# {id}\n", schema)
                is { } service)
                docs.Add(service);

        Assert.Equal(1 + (offering is null ? 0 : 1) + services.Length, docs.Count);

        var byId = docs.ToDictionary(d => d.Scalar("id"), d => d, StringComparer.OrdinalIgnoreCase);
        var found = new List<Finding>();

        new AlertAfterNeedsAnAlert().Check(new CorpusRuleContext(docs, byId, Empty, nfrs,
            new RuleSpec { Id = new RuleId("alert-after-needs-an-alert") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => Report(Sev.Error, at, c, m, l),
            (at, c, m, l) => Report(Sev.Warning, at, c, m, l)));
        return found;

        void Report(Sev severity, Doc at, CheckId check, string message, int? line)
            => found.Add(new Finding(at.Rel, line, severity, check, message));
    }

    // The rule reads the records and never the tree, so an empty corpus of files is the honest thing to
    // hand it.
    private static readonly Tree Empty = new(new HashSet<string>(StringComparer.Ordinal), _ => "");
}
