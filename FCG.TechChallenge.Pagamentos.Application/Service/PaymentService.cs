using FCG.TechChallenge.Pagamentos.Application.DTOs;
using FCG.TechChallenge.Pagamentos.Application.Interfaces;
using FCG.TechChallenge.Pagamentos.Application.Mappers;
using FCG.TechChallenge.Pagamentos.Domain.Entities;
using FCG.TechChallenge.Pagamentos.Domain.ValueObjects;

namespace FCG.TechChallenge.Pagamentos.Application.Service
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _repository = paymentRepository;    
        }

        public async Task<PaymentResponse> AuthorizeAsync(CreatePaymentRequest req, CancellationToken ct = default)
        {
            var money = new Money(req.Amount, req.Currency);

            var payment = Payment.Authorize(req.MerchantId, money, req.Method);
            await _repository.AddAsync(payment, ct);
            return PaymentMapper.ToResponse(payment);
        }

        public async Task<PaymentResponse> CaptureAsync(int id, CancellationToken ct = default)
        {
            var payment = await _repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Payment not found");
            payment.Capture();
            await _repository.SaveChangesAsync(ct);
            return PaymentMapper.ToResponse(payment);
        }

        public async Task<PaymentResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            return entity is null ? null : PaymentMapper.ToResponse(entity);
        }

        public async Task<PaymentResponse> RefundAsync(int id, CancellationToken ct = default)
        {
            var payment = await _repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Payment not found");
            payment.Refund();
            await _repository.SaveChangesAsync(ct);
            return PaymentMapper.ToResponse(payment);
        }
    }
}
