using FCG.TechChallenge.Pagamentos.Application.DTOs;
using FluentValidation;

namespace FCG.TechChallenge.Pagamentos.Application.Validator
{
    public sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
    {
        public CreatePaymentValidator()
        {
            RuleFor(x => x.MerchantId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }

    }
}
