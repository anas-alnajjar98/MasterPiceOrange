using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.DtosForGetallproduct
{
    public class AuctionProductDto
    {
        public int AuctionId { get; set; }
        public decimal? CurrentHighestBid { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public ProductDetailsDto ProductDetails { get; set; }
    }
}
