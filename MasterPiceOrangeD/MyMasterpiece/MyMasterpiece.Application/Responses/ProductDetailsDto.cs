using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class ProductDetailsDto
{
    public DateTime EndTime { get; set; }
    public decimal CurrentHighestBid { get; set; }
    public int TotalBids { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public decimal StartingPrice { get; set; }
    public string Country { get; set; }
    public string Brand { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
    public string HighestBidderName { get; set; }
}

}
