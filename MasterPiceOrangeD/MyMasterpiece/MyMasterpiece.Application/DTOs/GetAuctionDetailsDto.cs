﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class GetAuctionDetailsDto
    {
        public int AuctionId { get; set; }                  // Auction ID
        public string ProductName { get; set; }             // Product name
        public decimal CurrentHighestBid { get; set; }      // Current highest bid
        public decimal ShippingCost { get; set; } = 20;     // Example shipping cost
        public decimal Tax { get; set; } = 5;               // Example tax
        public decimal TotalAmount => CurrentHighestBid + ShippingCost + Tax;   // Total amount calculated
    }
}
