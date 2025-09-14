using FCG.TechChallenge.Pagamentos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
