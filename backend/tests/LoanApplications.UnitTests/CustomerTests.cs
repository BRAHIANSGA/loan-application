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
    [InlineData(0)]
    [InlineData(-1)]
    public void Register_NonPositiveAmount_ThrowsArgumentOutOfRangeException(int amount)
    {
        var request = TestData.ValidRequest() with { RequestedAmount = amount };

        Assert.Throws<ArgumentOutOfRangeException>(() => Customer.Register(request));
    }

    [Fact]
    public void Register_AmountWithMoreThanTwoDecimals_ThrowsArgumentException()
    {
        var request = TestData.ValidRequest() with { RequestedAmount = 1_000.125m };

        Assert.Throws<ArgumentException>(() => Customer.Register(request));
    }

    [Fact]
    public void Register_NamesWithExtraSpaces_StoresThemTrimmed()
    {
        var request = TestData.ValidRequest() with { FirstName = " Jane ", LastName = " Doe ", CompanyName = " Doe LLC " };

        var customer = Customer.Register(request);

        Assert.Equal(("Jane", "Doe", "Doe LLC"), (customer.FirstName, customer.LastName, customer.CompanyName));
    }
}
