# Loan Application

> **Video walkthrough:** _pending, will be added here before submission._

A small business loan application flow. A Next.js form sends the application to a .NET API,
a rule engine approves or denies it, approved applications are stored in PostgreSQL in a single
transaction, and a background worker syncs the customer and the application to an external
service over HTTP.

Design decisions are explained in [ARCHITECTURE.md](ARCHITECTURE.md).

## Run everything

Requirements: Docker. The first build downloads the .NET and Node images and takes a few minutes.

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Web app | http://localhost:3000 |
| API | `POST http://localhost:5080/api/loan-applications` |
| External service mock (what it received) | http://localhost:5090/customers |
| PostgreSQL | `localhost:5432`, user `postgres`, password `postgres` |

To see the background worker retry, start the mock with random failures (50% here):

```bash
FAILURE_RATE=0.5 docker compose up --build
```

### Run the apps outside Docker

Requirements: .NET 10 SDK and Node.js 22. The database and the mock still run in Docker.

```bash
docker compose up -d db mock
dotnet run --project backend/src/LoanApplications.Api
npm --prefix frontend install
npm --prefix frontend run dev
```

## Run the tests

Requirements: .NET 10 SDK and Docker running. Integration tests start a disposable PostgreSQL container.

```bash
dotnet test --solution backend/LoanApplications.slnx
```

- **Unit tests:** each rule, the rule engine (including a rule it has never seen), the `Ssn` value object and the `Customer` aggregate.
- **Integration tests:** the endpoint for approved, denied, returning-customer and invalid requests, the transaction rollback, a rule added only through dependency injection, and the outbox dispatcher.

## Test data

| Scenario | What to enter |
|---|---|
| Approved | Any state except New York and SSN `123-45-6789` |
| Denied by state | State **New York** |
| Denied by SSN blacklist | SSN `111-11-1111`, `222-22-2222` or `333-33-3333` |
| Returning customer | Submit `123-45-6789` again with a different amount or address |
| Rejected input | SSN `000-12-3456`, amount `0.50` or `10.555`; through the API also state `ZZ` |

After each approval, http://localhost:5090/customers shows what the external service received:
one record per customer, updated in place for returning customers.

The mock keeps its data in memory. To start over with an empty database and mock, run
`docker compose down` before `docker compose up`.
