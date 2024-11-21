using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Domain.Entities
{
    public class Auction
    {
        public int AuctionId { get; set; }        
        public DateTime StartTime { get; set; }      
        public DateTime EndTime { get; set; }         
        public decimal CurrentHighestBid { get; set; }
        public int? CurrentHighestBidderId { get; set; } 

        // Foreign Key for Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string AuctionStatus { get; set; } = "ongoing";

        public bool IsNotificationSent { get; set; } = false;
        public User CurrentHighestBidder { get; set; }  // Navigation property for highest bidder

        // Navigation Properties
        public ICollection<Bid> Bids { get; set; }
    }
}
