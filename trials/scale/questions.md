# The scale trial's questions

These are the questions the scale trial's question arm asks. They are drafted, and no session has run them. Each one
is answerable from the corpus (one repository of knowledge records) that [`generate.mjs`](generate.mjs) writes for 8
applications, and from no other source.

**The prose arm gets a cut of the corpus: the fixed records, Alder's records and Birch's records.** The `Cut` column
says whether the answer is in that cut. A question marked `out` measures what the prose arm loses at scale. A question
marked `in` measures whether a long text still answers what it contains.

A question needs scale where its answer is in one application's copy of a record that every application has, in a
clause of an imported policy, or across a cross-reference between records. The `Why at scale` column says which.

## The questions

| #   | Question, as a session receives it                                                                                                                    | Cut |
|-----|-------------------------------------------------------------------------------------------------------------------------------------------------------|-----|
| 1   | Where is the API contract for the Gorse application's payment ledger published?                                                                       | out |
| 2   | Who owns the Fir application's accepted departure from the rule that operating knowledge must not live in one person's head, and when is it reviewed? | out |
| 3   | How fast must a card authorisation in the Hazel application return, and how long does a breach last before it alerts?                                 | out |
| 4   | Only one engineer knows how to restore the Alder ledger, and they are always reachable. Is that acceptable here?                                      | in  |
| 5   | After the Damson checkout refused payments the card had already authorised, what recovery target was set for its ledger?                              | out |
| 6   | A customer of the Elder checkout sees two pending amounts on their statement for one order. Has this been solved before, and what do I do?            | out |
| 7   | How long does the Birch application keep a borrower linked to a returned loan, and does anything enforce that?                                        | in  |
| 8   | The Cedar application's hosted card payment page is down. What happens to a borrower's hold, and what is the fallback?                                | out |
| 9   | I am adding a refund endpoint to a payment service. What header must a refund request include, and how should the caller build its value?             | in  |
| 10  | In the Alder application, nuget.org returns 404 for a `kac` version released minutes ago. Is there a runbook, and how severe is it?                   | in  |
| 11  | How many services across the estate are graded critical and raise nothing when they fail? List them.                                                  | out |
| 12  | If the Alder application's search service goes down, which other services feel it directly?                                                           | in  |

## The answers

| #   | Known answer                                                                                                                                                        | Record ids                                    | Why at scale                                                           |
|-----|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------|------------------------------------------------------------------------|
| 1   | `https://git.example.com/example-estate/gorse-payment-ledger/blob/main/contract/openapi.yaml`                                                                       | `svc-gorse-payment-ledger`                    | one service among 8 copies                                             |
| 2   | `human:fen.roe`, reviewed by 2027-09-07. It departs from `eng:pol-KNOW.HEADS`.                                                                                      | `dev-fir-one-maintainer`                      | one deviation among 8 copies, whose `owner` alone differs between them |
| 3   | p95 under 800ms over a rolling hour, on `POST /authorisations`. It alerts after 15 minutes.                                                                         | `nfr-0022`                                    | a numbered id that says nothing of its application                     |
| 4   | No. `eng:pol-KNOW.HEADS` says critical operating or recovery knowledge **MUST NOT** exist only in someone's head. Being reachable does not meet it.                 | `eng:pol-KNOW.HEADS`                          | a clause among 276 in an imported policy                               |
| 5   | RPO 5 minutes and RTO 1 hour. The postmortem prompted the NFR.                                                                                                      | `pmt-0007`, `nfr-0011`                        | a cross-reference from a postmortem to an NFR, both numbered           |
| 6   | Yes. Void the later authorisation with the PSP, write the reversal to the ledger as its own entry, and derive the idempotency key from the order and the operation. | `fix-0017`                                    | one fix among 8 copies, found by its symptom                           |
| 7   | Two years after the item comes back, then the link is cleared. Nothing enforces it: the clearing script is run by hand.                                             | `dat-birch-loans`                             | one data record among 8 copies                                         |
| 8   | The hold is created and stays unpaid. Branch staff take the payment at the desk.                                                                                    | `itg-cedar-card-payments`                     | one integration among 8 copies                                         |
| 9   | An `Idempotency-Key` header. The caller sends the same key on every retry, and should derive it from the order and the operation.                                   | `std-IDEM.KEY`                                | a rule among 109 in the standards                                      |
| 10  | Yes, a sev3 runbook, which nobody has rehearsed.                                                                                                                    | `rbk-alder-nuget-404-on-publish`              | one runbook among 8 copies                                             |
| 11  | 8: the `kac` service of each application, `svc-alder-kac` to `svc-hazel-kac`.                                                                                       | `svc-alder-kac` to `svc-hazel-kac`            | a count over all 120 services                                          |
| 12  | `svc-alder-catalogue-web`, which lists it in `depends-on`.                                                                                                          | `svc-alder-search`, `svc-alder-catalogue-web` | a reverse lookup of the dependency graph                               |

## Grading each question

Each question becomes a `house` case, with a `task-` grader for each fact in the known answer and a `cite-` grader for
each record id. These are the patterns to start from. Check each against the kept replies before you trust it.

| #   | `task-` patterns                                        | `cite-` patterns                 |
|-----|---------------------------------------------------------|----------------------------------|
| 1   | `gorse-payment-ledger/blob/main/contract/openapi\.yaml` | `svc-gorse-payment-ledger`       |
| 2   | `fen\.roe`, `2027-09-07`                                | `dev-fir-one-maintainer`         |
| 3   | `800\s?ms`, `15 min`                                    | `nfr-0022`                       |
| 4   | `(?i)\bno\b\|not acceptable`                            | `pol-KNOW\.HEADS`                |
| 5   | `(?i)RPO\D{0,12}5 min`, `(?i)RTO\D{0,12}1 hour`         | `nfr-0011`                       |
| 6   | `(?i)void`, `(?i)order\W+(and\W+)?(the\W+)?operation`   | `fix-0017`                       |
| 7   | `(?i)two years\|2 years`, `(?i)by hand\|manual`         | `dat-birch-loans`                |
| 8   | `(?i)unpaid`, `(?i)desk`                                | `itg-cedar-card-payments`        |
| 9   | `Idempotency-Key`, `(?i)same key`                       | `std-IDEM`                       |
| 10  | `(?i)sev\s?3`                                           | `rbk-alder-nuget-404-on-publish` |
| 11  | `\b8\b\|(?i)eight`                                      | `svc-hazel-kac`, `svc-alder-kac` |
| 12  | `catalogue-web`                                         | `svc-alder-catalogue-web`        |
