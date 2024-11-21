using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class PlaceBids
    {
        public int AuctionId { get; set; }
        public decimal BidAmount { get; set; }
        public int UserId { get; set; }
    }
}
