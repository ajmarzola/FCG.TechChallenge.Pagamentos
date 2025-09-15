using FCG.TechChallenge.Pagamentos.Domain.Entities;

namespace FCG.TechChallenge.Pagamentos.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment, CancellationToken ct = default);
        Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
