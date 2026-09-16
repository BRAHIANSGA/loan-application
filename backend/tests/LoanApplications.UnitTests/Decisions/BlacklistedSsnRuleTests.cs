using LoanApplications.Domain;
using LoanApplications.Domain.Decisions.Rules;

namespace LoanApplications.UnitTests.Decisions;

public sealed class BlacklistedSsnRuleTests
{
    private readonly BlacklistedSsnRule _rule = new(new FakeSsnBlacklist("111-11-1111"));

    [Theory]
    [InlineData("111-11-1111")]
    [InlineData("111111111")]
    public async Task EvaluateAsync_BlacklistedSsn_ReturnsSsnBlacklisted(string ssn)
    {
        var request = TestData.ValidRequest() with { Ssn = Ssn.Parse(ssn) };

        var reason = await _rule.EvaluateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(BlacklistedSsnRule.Reason, reason);
    }

    [Fact]
    public async Task EvaluateAsync_SsnNotBlacklisted_ReturnsNull()
    {
        var reason = await _rule.EvaluateAsync(TestData.ValidRequest(), TestContext.Current.CancellationToken);

        Assert.Null(reason);
    }
}
