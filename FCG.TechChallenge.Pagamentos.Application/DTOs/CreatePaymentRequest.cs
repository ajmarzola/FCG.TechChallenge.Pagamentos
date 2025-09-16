using FCG.TechChallenge.Pagamentos.Domain.Enums;

namespace FCG.TechChallenge.Pagamentos.Application.DTOs
{
    public sealed record CreatePaymentRequest
    (
        string MerchantId,
        decimal Amount,
        string Currency,
        PaymentMethod Method
    );    
}
