using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AuctionDto
{
    public class CreateAuctionDto
    {
        public int ProductId { get; set; }
        public int DurationHours { get; set; }
        public int DurationMinutes { get; set; }
    }

}
