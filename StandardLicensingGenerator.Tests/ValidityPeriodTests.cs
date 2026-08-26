using StandardLicensingGenerator.Models;
using Xunit;

namespace StandardLicensingGenerator.Tests;

public class ValidityPeriodTests
{
    private static readonly DateTime From = new(2026, 1, 15);

    [Theory]
    [InlineData("30", 2026, 2, 14)]        // bare number = days
    [InlineData("45 days", 2026, 3, 1)]
    [InlineData("1 d", 2026, 1, 16)]
    [InlineData("2 weeks", 2026, 1, 29)]
    [InlineData("1 month", 2026, 2, 15)]
    [InlineData("6 mo", 2026, 7, 15)]
    [InlineData("1 year", 2027, 1, 15)]
    [InlineData("5 Years", 2031, 1, 15)]   // case-insensitive
    [InlineData(" 1 yr ", 2027, 1, 15)]    // whitespace tolerated
    public void TryCompute_ValidInput_ComputesExpectedDate(string text, int year, int month, int day)
    {
        Assert.True(ValidityPeriod.TryCompute(text, From, out var expiration));
        Assert.Equal(new DateTime(year, month, day), expiration);
    }

    [Fact]
    public void TryCompute_MonthEndClamps()
    {
        Assert.True(ValidityPeriod.TryCompute("1 month", new DateTime(2026, 1, 31), out var expiration));
        Assert.Equal(new DateTime(2026, 2, 28), expiration);
    }

    [Theory]
    [InlineData("")]
    [InlineData("soon")]
    [InlineData("0 days")]
    [InlineData("-5 days")]
    [InlineData("5 fortnights")]
    [InlineData("1 month 2 days")]
    [InlineData("9999 years")] // past DateTime.MaxValue
    public void TryCompute_InvalidInput_ReturnsFalse(string text)
    {
        Assert.False(ValidityPeriod.TryCompute(text, From, out _));
    }
}
