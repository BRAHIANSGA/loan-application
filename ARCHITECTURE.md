# Architecture

## Structure

| Path | Responsibility |
|---|---|
| `backend/src/LoanApplications.Domain` | `Customer` aggregate, `LoanApplication`, value objects (`Ssn`, `Address`), rule engine and rules. No dependencies. |
| `backend/src/LoanApplications.Application` | `SubmitLoanApplication` use case, its ports, and the contract used to sync customers with the external service. |
| `backend/src/LoanApplications.Infrastructure` | EF Core + PostgreSQL, the outbox and its background worker, the HTTP client, the SSN blacklist. |
| `backend/src/LoanApplications.Api` | One Minimal API endpoint and the composition root. |
| `external-service-mock` | Stand-in for a third-party service. Shares no code with the backend. |
| `frontend` | Next.js form and result pages. |

Dependencies point inward: `Api -> Application -> Domain`, and `Infrastructure` implements the
interfaces of the inner layers. Separate projects make the compiler enforce it.

Every interface has one reason to exist:

| Interface | Why |
|---|---|
| `ICustomerRepository`, `IOutbox`, `IUnitOfWork` | `Application` cannot reference EF Core. One scoped `DbContext` implements the three, and the use case decides when to commit because the customer and the event must be saved together. |
| `IExternalCustomerClient` | Contract of the background handler, not of the use case. It sits in `Application` next to the snapshots it sends, so `Infrastructure` implements it and the tests replace it with a fake. |
| `IDenialRule` | New rules are added without editing existing ones. |
| `ISsnBlacklist` | The domain cannot read configuration, and the list can move to a table or a fraud service later. |

## Rule engine

`LoanDecisionEngine` receives every registered `IDenialRule`, runs all of them and approves the
application only if none returns a `DenialReason`. It returns every matched reason, which keeps
the decision auditable.

| Rule | Denial code |
|---|---|
| `RestrictedStateRule` | `STATE_NOT_ELIGIBLE` (state is `NY`) |
| `BlacklistedSsnRule` | `SSN_BLACKLISTED` (SSN is in `SsnBlacklist` in `appsettings.json`) |

**Adding a rule** touches no existing code:

1. Create a class in `Domain/Decisions/Rules` that implements `IDenialRule` and returns its own
   `DenialReason` code.
2. Register it in `Program.cs`: `builder.Services.AddScoped<IDenialRule, YourRule>();`

`DenialReason` is a string code rather than an enum on purpose: an enum would have to be edited
for every new rule. The frontend shows a specific message only for reasons that are safe to share
(the state restriction) and a generic one for anything else, including rules it does not know.
The test `Submit_RuleRegisteredOnlyInDependencyInjection_DeniesWithItsReason` proves the flow end to end.

## Transaction and background event

```
POST /api/loan-applications
  -> rule engine -- denied --> 200 { decision: Denied }   (nothing is stored)
  -> approved:
       find customer by SSN -> register, or update customer and loan application
       add outbox message (Create | Update) with a snapshot of both
       SaveChanges -> one database transaction
  -> 200 { decision: Approved }

OutboxWorker (BackgroundService, every 2 s)
  -> oldest pending messages -> POST or PUT to the external service -> delete the message
```

The repository, the outbox and the unit of work share one scoped `DbContext`, so a single
`SaveChanges` commits the customer, the loan application and the event together. The HTTP call
never runs inside the request or the transaction: that is the transactional outbox pattern.

| If this fails | Result |
|---|---|
| Rule engine, validation or the database write | Nothing is committed: no customer, no application, no event. An integration test makes the outbox insert fail inside the transaction and checks that the three tables stay empty. |
| External service, briefly | `AddStandardResilienceHandler()` retries the call with exponential backoff. |
| External service, for longer | The message is retried on every tick. After 5 failed attempts the worker gives up on it and logs an error. The row keeps its last error until an operator queues it again with `UPDATE "OutboxMessages" SET "Attempts" = 0`. Until then that customer is missing from the external service. |
| The API restarts between commit and dispatch | The message is in the database and is sent after the restart. |
| One message keeps failing | The worker stops the batch there, so an `Update` never overtakes its `Create`. The price is availability: no other customer is synced until that message succeeds or gives up. Blocking only the failing customer is the next step. |

Delivery is at-least-once. The external contract is idempotent: `POST /customers` answers `409`
for a known id and the worker treats that as done; `PUT /customers/{id}` overwrites.

### External service contract

| Endpoint | Purpose |
|---|---|
| `POST /customers` | New customer. `200`, or `409` if the id already exists. |
| `PUT /customers/{id}` | Returning customer. `200`, or `404` if unknown. |
| `GET /customers` | What the service received, with masked SSNs. |

The payload carries the customer, its address, the SSN and a nested `loanApplication`
(`id`, `requestedAmount`). Our `customerId` is the external key, so a returning customer
always updates the same record.

## Input validation

The domain owns every bound: `LoanApplication.IsValidRequestedAmount`, `Ssn.IsValid`,
`UsStates.IsValid`, `Address.IsValidZipCode`, `PlainText.IsValid` and the maximum lengths declared
on `Customer` and `Address`. The API attributes call them to answer `400` with field errors before
the use case runs, and the EF Core column lengths use the same constants. The form checks only
format and length, for immediate feedback. Rules that need domain knowledge, such as which SSNs the
SSA issues, live on the server alone, and its field errors are shown next to each input.

| Field | Rule |
|---|---|
| Requested amount | From $1 to $1,000,000,000, at most two decimals. The upper bound is a sanity limit; a credit limit would be a denial rule. |
| SSN | `123-45-6789` or `123456789`, and a number the SSA issues: no area 000, 666 or 900-999, no group 00, no serial 0000. |
| Address | Street and city trimmed and single-line. The state must be one of the 50 states or DC. ZIP `12345` or `12345-6789`. |
| Names and company | Required, trimmed and single-line (no control characters), up to 100 and 200 characters. |

## Database schema

The schema lives in code: the EF Core migrations in `Infrastructure/Persistence/Migrations`, applied
when the API starts. This prints the SQL they run (requires the `dotnet-ef` tool):

```bash
dotnet ef migrations script --project backend/src/LoanApplications.Infrastructure --startup-project backend/src/LoanApplications.Api
```

| Table | Notes |
|---|---|
| `Customers` | Personal data. Unique index on `Ssn`. The address is stored in `Address_*` columns (an EF Core complex type). |
| `LoanApplications` | `Id`, `RequestedAmount`, `CustomerId`. A unique index on `CustomerId` enforces one application per customer in the database as well. |
| `OutboxMessages` | `Operation`, `Payload` (`jsonb`), `OccurredAt`, `Attempts`, `LastError`. A row is deleted once its message is delivered, so the table only holds pending work. |

## Data protection

The SSN is stored as nine digits in `Customers`. The outbox payload carries it too, because the
external service needs it, and that row is deleted as soon as the message is delivered. `Ssn.ToString()` and `CustomerSnapshot.ToString()`
mask it so it cannot leak through logs, the API never returns it, and it never appears in a URL: the
denied page receives only denial codes that are safe to show. The mock masks the SSN on arrival.

## Trade-offs

| Left out | Why |
|---|---|
| Storing denied applications | Not required. A production lender would keep them for audit and adverse action notices. |
| Blacklist in a database table | Three values. `ISsnBlacklist` allows moving it without touching the rule. |
| MediatR, CQRS, generic repositories, a messaging framework | One use case and one event do not need them. The outbox is about 60 lines. |
| Several workers, backoff and alerts for the outbox | One worker, five attempts and a manual reset are enough here. Production would add `FOR UPDATE SKIP LOCKED`, exponential backoff, per-customer blocking and an alert when a message gives up. |
| Two submissions with the same SSN at the same instant | New customer: the unique index keeps one; the second request fails with `500` and can be retried. Returning customer: both succeed and the last write wins. |
| Encryption of the SSN at rest | Out of scope. Production would use column encryption or a KMS. |
| Address verification, a second address line, US territories | Format and state code are validated. Confirming that an address exists needs an external provider. |
| Migrations as a deployment step | Applied at startup for a one-command setup. |
| Authentication | Excluded by the assignment. |
| Frontend and browser end-to-end tests | The backend validates every request; the video covers the full flow. |
| Credentials in `appsettings.json` and `docker-compose.yml` | Local defaults only. Production would read them from a secret store. |
