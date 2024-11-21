using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class CreateAuctionDto
    {
        public int ProductId { get; set; }
        public decimal StartingPrice { get; set; }
        public int DurationHours { get; set; }
    }
}
