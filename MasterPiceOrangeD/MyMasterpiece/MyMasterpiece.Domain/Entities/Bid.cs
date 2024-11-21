using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Domain.Entities
{
    public class Bid
    {
        public int BidId { get; set; }                // Primary Key
        public decimal BidAmount { get; set; }        // Amount of the bid
        public DateTime BidTime { get; set; } = DateTime.Now; // Time the bid was placed

        // Foreign Key for Auction
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        // Foreign Key for User (Bidder)
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
