using CartService.Domain.ValueObjects;

namespace CartService.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldCreateMoney_WhenDataIsValid()
    {
        var money = Money.Create(25, "USD");

        Assert.Equal(25, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenAmountIsNegative()
    {
        Assert.Throws<ArgumentException>(() => Money.Create(-1, "USD"));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenCurrencyIsInvalid()
    {
        Assert.Throws<ArgumentException>(() => Money.Create(25, "US"));
    }
}
