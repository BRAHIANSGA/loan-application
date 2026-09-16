using LoanApplications.Domain;

namespace LoanApplications.UnitTests;

public sealed class SsnTests
{
    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123456789")]
    [InlineData(" 123-45-6789 ")]
    public void Parse_ValidFormat_StoresDigitsOnly(string input)
    {
        var ssn = Ssn.Parse(input);

        Assert.Equal("123456789", ssn.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123-45-678")]
    [InlineData("1234567890")]
    [InlineData("1-2-3-4-5-6-7-8-9")]
    [InlineData("abc-de-fghi")]
    public void Parse_InvalidFormat_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => Ssn.Parse(input));
    }

    [Fact]
    public void ToString_AnySsn_MasksAllButLastFourDigits()
    {
        var ssn = Ssn.Parse("123-45-6789");

        Assert.Equal("***-**-6789", ssn.ToString());
    }
}
