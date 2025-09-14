using FCG.TechChallenge.Pagamentos.Domain.Enums;
using FCG.TechChallenge.Pagamentos.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCG.TechChallenge.Pagamentos.Domain.Entities
{
    public sealed class Payment
    {
        private Payment() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string MerchantId { get; private set; } = string.Empty;

        [Required]
        public Money Amount { get; private set; } = Money.Zero("BRL"); 

        [Required]
        [Column(TypeName = "int")]
        public PaymentMethod Method { get; private set; } = PaymentMethod.Card;

        [Required]
        [Column(TypeName = "int")] 
        public PaymentStatus Status { get; private set; } = PaymentStatus.Authorized;

        [Required]
        [Column(TypeName = "datetime2")] 
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? CapturedAtUtc { get; private set; }

        [Column(TypeName = "datetime2")]
        public DateTime? RefundedAtUtc { get; private set; }

        public static Payment Authorize(string merchantId, Money amount, PaymentMethod method)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                throw new ArgumentException("MerchantId required");

            if (amount.Amount <= 0)
                throw new ArgumentException("Amount must be > 0");

            return new Payment
            {
                MerchantId = merchantId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Authorized
            };
        }

        public void Capture()
        {
            if (Status != PaymentStatus.Authorized)
                throw new InvalidOperationException("Only authorized can be captured");

            Status = PaymentStatus.Captured;
            CapturedAtUtc = DateTime.UtcNow;
        }

        public void Refund()
        {
            if (Status != PaymentStatus.Captured)
                throw new InvalidOperationException("Only captured can be refunded");

            Status = PaymentStatus.Refunded;
            RefundedAtUtc = DateTime.UtcNow;
        }
    }
}
