namespace LoanApplications.Domain;

public sealed class Customer
{
    public const int MaxNameLength = 100;
    public const int MaxCompanyNameLength = 200;

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string CompanyName { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Ssn Ssn { get; private set; } = null!;
    public LoanApplication LoanApplication { get; private set; } = null!;

    private Customer()
    {
    }

    public static Customer Register(LoanRequest request)
    {
        var customer = new Customer { Id = Guid.CreateVersion7(), Ssn = request.Ssn };
        customer.LoanApplication = LoanApplication.Create(customer.Id, request.RequestedAmount);
        customer.CopyDetailsFrom(request);
        return customer;
    }

    public void UpdateDetails(LoanRequest request)
    {
        CopyDetailsFrom(request);
        LoanApplication.ChangeRequestedAmount(request.RequestedAmount);
    }

    private void CopyDetailsFrom(LoanRequest request)
    {
        FirstName = PlainText.Required(request.FirstName, MaxNameLength, nameof(request.FirstName));
        LastName = PlainText.Required(request.LastName, MaxNameLength, nameof(request.LastName));
        CompanyName = PlainText.Required(request.CompanyName, MaxCompanyNameLength, nameof(request.CompanyName));
        Address = request.Address;
    }
}
