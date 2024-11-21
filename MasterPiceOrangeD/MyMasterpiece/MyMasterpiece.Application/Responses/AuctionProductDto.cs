using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class AuctionProductDto
    {
        public int AuctionId { get; set; }
        public DateTime EndTime { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public string HighestBidderName { get; set; }
    }
}
