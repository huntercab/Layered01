using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Domain.ValueObject
{
    public class Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = null!;

        private Money()
        {
            //Required by EF Core
        }

        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required", nameof(currency));

            currency = currency.Trim().ToUpperInvariant();

            if (currency.Length != 3)
                throw new ArgumentException("Expecting ISO format", nameof(currency));

            Amount = amount;
            Currency = currency;
        }
    }
}
