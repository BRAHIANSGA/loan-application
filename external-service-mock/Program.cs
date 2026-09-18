using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var customers = new ConcurrentDictionary<Guid, Customer>();
var failureRate = app.Configuration.GetValue<double>("FailureRate");

app.Use(async (context, next) =>
{
    if (!HttpMethods.IsGet(context.Request.Method) && Random.Shared.NextDouble() < failureRate)
    {
        app.Logger.LogWarning("Simulated outage for {Method} {Path}", context.Request.Method, context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return;
    }

    await next(context);
});

app.MapPost("/customers", (Customer customer) =>
{
    if (!customers.TryAdd(customer.Id, customer))
    {
        return Results.Conflict();
    }

    app.Logger.LogInformation("Created {Customer}", customer);
    return Results.Ok();
});

app.MapPut("/customers/{id:guid}", (Guid id, Customer customer) =>
{
    if (!customers.ContainsKey(id))
    {
        return Results.NotFound();
    }

    customers[id] = customer;
    app.Logger.LogInformation("Updated {Customer}", customer);
    return Results.Ok();
});

app.MapGet("/customers", () => customers.Values);

app.Run();

internal sealed record Customer(
    Guid Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Ssn,
    Address Address,
    LoanApplication LoanApplication)
{
    // Last four digits only, in logs and in responses.
    public string Ssn { get; } = $"***-**-{Ssn[^4..]}";
}

internal sealed record Address(string Street, string City, string State, string ZipCode);

internal sealed record LoanApplication(Guid Id, decimal RequestedAmount);
