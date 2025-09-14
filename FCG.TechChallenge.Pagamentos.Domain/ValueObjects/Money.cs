using Microsoft.EntityFrameworkCore;

namespace FCG.TechChallenge.Pagamentos.Domain.ValueObjects
{
    [Owned]
    public sealed record Money(decimal Amount, string Currency)
    {
        public static Money Zero(string currency) => new(0m, currency);
        public override string ToString() => $"{Amount:0.00} {Currency}";
    }
}
