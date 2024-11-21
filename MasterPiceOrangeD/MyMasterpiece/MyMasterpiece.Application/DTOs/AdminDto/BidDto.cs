using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AdminDto
{
    public class BidDto
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public decimal BidAmount { get; set; }
        public DateTime BidTime { get; set; }
    }

}
