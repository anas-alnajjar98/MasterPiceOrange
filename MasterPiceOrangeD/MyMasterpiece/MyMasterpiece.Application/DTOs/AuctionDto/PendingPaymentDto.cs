using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AuctionDto
{
    public class PendingPaymentDto
    {
        public int PaymentId { get; set; }
        public int AuctionId { get; set; }
        public string ProductName { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string UserEmail { get; set; }
        public string Username { get; set; }
        public DateTime RescheduledEndTime { get; set; }
    }

}
