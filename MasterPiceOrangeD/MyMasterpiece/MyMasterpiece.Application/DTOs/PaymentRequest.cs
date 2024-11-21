using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class PaymentRequest
    {
        public int AuctionId { get; set; }
        public decimal Amount { get; set; }
    }
}
