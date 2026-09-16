using LoanApplications.Domain.Decisions.Rules;

namespace LoanApplications.UnitTests.Decisions;

public sealed class RestrictedStateRuleTests
{
    private readonly RestrictedStateRule _rule = new();

    [Theory]
    [InlineData("NY")]
    [InlineData("ny")]
    [InlineData(" Ny ")]
    public async Task EvaluateAsync_ApplicantFromNewYork_ReturnsStateNotEligible(string state)
    {
        var request = TestData.ValidRequest() with { Address = TestData.AddressIn(state) };

        var reason = await _rule.EvaluateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(RestrictedStateRule.Reason, reason);
    }

    [Theory]
    [InlineData("TX")]
    [InlineData("NJ")]
    public async Task EvaluateAsync_ApplicantFromAnotherState_ReturnsNull(string state)
    {
        var request = TestData.ValidRequest() with { Address = TestData.AddressIn(state) };

        var reason = await _rule.EvaluateAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(reason);
    }
}
