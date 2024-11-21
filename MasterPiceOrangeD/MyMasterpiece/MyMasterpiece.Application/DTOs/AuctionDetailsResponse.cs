using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class AuctionDetailsResponse
    {
        public int AuctionId { get; set; }                  // ID of the auction
        public string ProductName { get; set; }             // Name of the product being auctioned
        public decimal CurrentHighestBid { get; set; }      // Current highest bid amount
        public decimal TotalAmount { get; set; }// Total amount including bid, tax, etc.
        public string image { get; set; }
    }
}
