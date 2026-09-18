using LoanApplications.Domain;

namespace LoanApplications.UnitTests;

public sealed class CustomerTests
{
    [Fact]
    public void Register_NewCustomer_CreatesItsLoanApplication()
    {
        var request = TestData.ValidRequest();

        var customer = Customer.Register(request);

        Assert.Equal(customer.Id, customer.LoanApplication.CustomerId);
        Assert.Equal(request.RequestedAmount, customer.LoanApplication.RequestedAmount);
    }

    [Fact]
    public void UpdateDetails_ReturningCustomer_UpdatesTheExistingLoanApplication()
    {
        var customer = Customer.Register(TestData.ValidRequest());
        var originalApplicationId = customer.LoanApplication.Id;
        var newRequest = TestData.ValidRequest() with { CompanyName = "Doe Catering LLC", RequestedAmount = 75_000m };

        customer.UpdateDetails(newRequest);

        Assert.Equal(originalApplicationId, customer.LoanApplication.Id);
        Assert.Equal(75_000m, customer.LoanApplication.RequestedAmount);
        Assert.Equal("Doe Catering LLC", customer.CompanyName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1_000_000_000)]
    public void Register_AmountAtTheBounds_IsAccepted(int amount)
    {
        var request = TestData.ValidRequest() with { RequestedAmount = amount };

        var customer = Customer.Register(request);

        Assert.Equal(amount, customer.LoanApplication.RequestedAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.5)]
    [InlineData(1_000.125)]
    [InlineData(1_000_000_000.01)]
    public void Register_AmountOutOfBounds_ThrowsArgumentOutOfRangeException(double amount)
    {
        var request = TestData.ValidRequest() with { RequestedAmount = (decimal)amount };

        Assert.Throws<ArgumentOutOfRangeException>(() => Customer.Register(request));
    }

    [Fact]
    public void Register_NamesWithExtraSpaces_StoresThemTrimmed()
    {
        var request = TestData.ValidRequest() with { FirstName = " Jane ", LastName = " Doe ", CompanyName = " Doe LLC " };

        var customer = Customer.Register(request);

        Assert.Equal(("Jane", "Doe", "Doe LLC"), (customer.FirstName, customer.LastName, customer.CompanyName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_BlankName_ThrowsArgumentException(string lastName)
    {
        var request = TestData.ValidRequest() with { LastName = lastName };

        Assert.Throws<ArgumentException>(() => Customer.Register(request));
    }

    [Fact]
    public void Register_CompanyNameTooLong_ThrowsArgumentOutOfRangeException()
    {
        var request = TestData.ValidRequest() with { CompanyName = new string('a', Customer.MaxCompanyNameLength + 1) };

        Assert.Throws<ArgumentOutOfRangeException>(() => Customer.Register(request));
    }

    [Theory]
    [InlineData("Jane\0Doe")]
    [InlineData("Jane\nDoe")]
    public void Register_NameWithControlCharacter_ThrowsArgumentException(string firstName)
    {
        var request = TestData.ValidRequest() with { FirstName = firstName };

        Assert.Throws<ArgumentException>(() => Customer.Register(request));
    }
}
