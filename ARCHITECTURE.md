# Architecture

## Structure

| Path | Responsibility |
|---|---|
| `backend/src/LoanApplications.Domain` | `Customer` aggregate, `LoanApplication`, value objects (`Ssn`, `Address`), rule engine and rules. No dependencies. |
| `backend/src/LoanApplications.Application` | `SubmitLoanApplication` use case and the ports it needs (`ICustomerRepository`, `IOutbox`, `IUnitOfWork`, `IExternalCustomerClient`). |
| `backend/src/LoanApplications.Infrastructure` | EF Core + PostgreSQL, the outbox and its background worker, the HTTP client, the SSN blacklist. |
| `backend/src/LoanApplications.Api` | One Minimal API endpoint and the composition root. |
| `external-service-mock` | Stand-in for a third-party service. Shares no code with the backend. |
| `frontend` | Next.js form and result pages. |

Dependencies point inward: `Api → Application → Domain`, and `Infrastructure` implements the
interfaces of the inner layers. Separate projects make the compiler enforce it.

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
  → rule engine ── denied ──→ 200 { decision: Denied }   (nothing is stored)
  → approved:
      find customer by SSN → register, or update customer and loan application
      add outbox message (Create | Update) with a snapshot of both
      SaveChanges → one database transaction
  → 200 { decision: Approved }

OutboxWorker (BackgroundService, every 2 s)
  → oldest pending messages → POST or PUT to the external service → mark as processed
```

The repository, the outbox and the unit of work share one scoped `DbContext`, so a single
`SaveChanges` commits the customer, the loan application and the event together. The HTTP call
never runs inside the request or the transaction: that is the transactional outbox pattern.

| If this fails | Result |
|---|---|
| Rule engine, validation or the database write | Nothing is committed: no customer, no application, no event. |
| External service, briefly | `AddStandardResilienceHandler()` retries the call with exponential backoff. |
| External service, longer | The message stays pending and is retried on the next tick. After 5 attempts it is kept with its last error. |
| The API restarts between commit and dispatch | The message is in the database and is sent after the restart. |
| A message fails | The worker stops the batch there, so an `Update` never reaches the service before its `Create`. |

Delivery is at-least-once. The external contract is idempotent: `POST /customers` answers `409`
for a known id and the worker treats that as done; `PUT /customers/{id}` overwrites.

## External service contract

| Endpoint | Purpose |
|---|---|
| `POST /customers` | New customer. `200`, or `409` if the id already exists. |
| `PUT /customers/{id}` | Returning customer. `200`, or `404` if unknown. |
| `GET /customers` | What the service received, with masked SSNs. |

The payload carries the customer, its address, the SSN and a nested `loanApplication`
(`id`, `requestedAmount`). Our `customerId` is the external key, so a returning customer
always updates the same record.

## Data protection

The SSN is stored as nine digits, masked by `Ssn.ToString()` so it cannot leak through logs, never
returned by the API and never placed in a URL. The mock masks it on arrival.

## Trade-offs

| Left out | Why |
|---|---|
| Storing denied applications | Not required. A production lender would keep them for audit and adverse action notices. |
| Blacklist in a database table | Three values. `ISsnBlacklist` allows moving it to a table or a fraud service without touching the rule. |
| `FOR UPDATE SKIP LOCKED` in the worker | One worker instance. It is the next step to run several API replicas. |
| MassTransit, Wolverine, CAP | A messaging framework for one event. The outbox is about 60 lines. |
| MediatR, CQRS, generic repositories | One use case does not need them. |
| Encryption of the SSN at rest | Out of scope. Production would use column encryption or a KMS. |
| Migrations as a deployment step | Applied at startup for a one-command setup. |
| Two new applications with the same SSN at the same instant | The unique index keeps one customer; the second request fails and can be retried. |
| Authentication | Excluded by the assignment. |
| Frontend and browser end-to-end tests | The backend validates every request; the video covers the full flow. |
| Credentials in `appsettings.json` and `docker-compose.yml` | Local defaults only. Production would read them from a secret store. |
