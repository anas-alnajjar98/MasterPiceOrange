using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class BidResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? AuctionId { get; set; }
        public decimal? HighestBid { get; set; }
        public int? HighestBidderId { get; set; }
    }
}
