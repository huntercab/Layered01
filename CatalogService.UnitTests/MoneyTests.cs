using CatalogService.Domain.ValueObject;

namespace CatalogService.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreateMoney_WhenValuesAreValid()
    {
        var money = new Money(25.25m, "USD");

        Assert.Equal(25.25m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_ShoudlThrowException_WhenAmountIsInvalid(decimal amount)
    {
        Assert.Throws<ArgumentException>(() => new Money(amount, "USD"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Constructor_ShoudlThrowException_WhenCurrencyIsInvalid(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(25.25m, currency));
    }
}
