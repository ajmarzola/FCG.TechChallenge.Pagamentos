using FCG.TechChallenge.Pagamentos.Application.DTOs;

namespace FCG.TechChallenge.Pagamentos.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> AuthorizeAsync(CreatePaymentRequest req, CancellationToken ct = default);
        Task<PaymentResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<PaymentResponse> CaptureAsync(int id, CancellationToken ct = default);
        Task<PaymentResponse> RefundAsync(int id, CancellationToken ct = default);
    }
}
