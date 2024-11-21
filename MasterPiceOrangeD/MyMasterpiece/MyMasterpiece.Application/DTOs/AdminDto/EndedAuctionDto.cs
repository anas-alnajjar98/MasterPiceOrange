using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AdminDto
{
    public class EndedAuctionDto
    {
        public int AuctionId { get; set; }
        public string ProductName { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public BidderDto HighestBidder { get; set; }
    }
}
