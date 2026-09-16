using LoanApplications.Domain;

namespace LoanApplications.UnitTests;

public sealed class AddressTests
{
    [Fact]
    public void Constructor_ValuesWithExtraSpaces_StoresThemNormalized()
    {
        var address = new Address("  9 Oak St ", " Austin ", " tx ", " 73301 ");

        Assert.Equal(new Address("9 Oak St", "Austin", "TX", "73301"), address);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("Texas")]
    [InlineData("")]
    public void Constructor_UnknownState_ThrowsArgumentException(string state)
    {
        Assert.Throws<ArgumentException>(() => new Address("9 Oak St", "Austin", state, "73301"));
    }

    [Theory]
    [InlineData("7330")]
    [InlineData("73301-12")]
    [InlineData("ABCDE")]
    public void Constructor_InvalidZipCode_ThrowsArgumentException(string zipCode)
    {
        Assert.Throws<ArgumentException>(() => new Address("9 Oak St", "Austin", "TX", zipCode));
    }

    [Fact]
    public void Constructor_BlankStreet_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Address("  ", "Austin", "TX", "73301"));
    }
}
