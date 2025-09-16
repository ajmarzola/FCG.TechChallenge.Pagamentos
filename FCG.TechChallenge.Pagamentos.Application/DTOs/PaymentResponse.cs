using FCG.TechChallenge.Pagamentos.Domain.Enums;

namespace FCG.TechChallenge.Pagamentos.Application.DTOs
{
    public sealed record PaymentResponse
    (
        int Id,
        string MerchantId,
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        PaymentStatus Status,
        DateTime CreatedAtUtc,
        DateTime? CapturedAtUtc,
        DateTime? RefundedAtUtc
    );    
}
