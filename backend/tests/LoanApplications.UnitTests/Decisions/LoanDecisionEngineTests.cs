using LoanApplications.Domain;
using LoanApplications.Domain.Decisions;
using LoanApplications.Domain.Decisions.Rules;

namespace LoanApplications.UnitTests.Decisions;

public sealed class LoanDecisionEngineTests
{
    [Fact]
    public async Task DecideAsync_NoRuleMatches_Approves()
    {
        var decision = await CreateEngine().DecideAsync(TestData.ValidRequest(), TestContext.Current.CancellationToken);

        Assert.True(decision.IsApproved);
    }

    [Fact]
    public async Task DecideAsync_SeveralRulesMatch_ReturnsEveryDenialReason()
    {
        var request = TestData.ValidRequest() with
        {
            Address = TestData.AddressIn("NY"),
            Ssn = Ssn.Parse("111-11-1111"),
        };

        var decision = await CreateEngine().DecideAsync(request, TestContext.Current.CancellationToken);

        Assert.False(decision.IsApproved);
        Assert.Equal(new[] { RestrictedStateRule.Reason, BlacklistedSsnRule.Reason }, decision.DenialReasons);
    }

    [Fact]
    public async Task DecideAsync_RuleAddedWithoutChangingExistingOnes_AppliesTheNewRule()
    {
        var engine = CreateEngine(new ExcludedCompanyRule("Acme Shell Co"));
        var request = TestData.ValidRequest() with { CompanyName = "Acme Shell Co" };

        var decision = await engine.DecideAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { ExcludedCompanyRule.Reason }, decision.DenialReasons);
    }

    private static LoanDecisionEngine CreateEngine(params IDenialRule[] additionalRules) =>
        new([new RestrictedStateRule(), new BlacklistedSsnRule(new FakeSsnBlacklist("111-11-1111")), .. additionalRules]);

    private sealed class ExcludedCompanyRule(string companyName) : IDenialRule
    {
        public static readonly DenialReason Reason = new("COMPANY_NOT_ELIGIBLE");

        public Task<DenialReason?> EvaluateAsync(LoanRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(request.CompanyName == companyName ? Reason : null);
    }
}
