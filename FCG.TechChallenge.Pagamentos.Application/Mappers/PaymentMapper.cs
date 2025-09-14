using FCG.TechChallenge.Pagamentos.Application.DTOs;
using FCG.TechChallenge.Pagamentos.Domain.Entities;

namespace FCG.TechChallenge.Pagamentos.Application.Mappers
{
    public static class PaymentMapper
    {
        public static PaymentResponse ToResponse(this Payment p) =>
            new PaymentResponse(
                p.Id,
                p.MerchantId,
                p.Amount.Amount,
                p.Amount.Currency,
                p.Method,
                p.Status,
                p.CreatedAtUtc,
                p.CapturedAtUtc,   
                p.RefundedAtUtc
            );
    }
}
