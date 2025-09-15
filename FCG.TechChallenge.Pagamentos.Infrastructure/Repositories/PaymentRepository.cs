using FCG.TechChallenge.Pagamentos.Application.Interfaces;
using FCG.TechChallenge.Pagamentos.Domain.Entities;
using FCG.TechChallenge.Pagamentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.TechChallenge.Pagamentos.Infrastructure.Repositories
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        protected readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
        {
            await _context.Payments.AddAsync(payment, ct);
            await SaveChangesAsync(ct);
            return payment;
        }

        public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }       
    }
}
