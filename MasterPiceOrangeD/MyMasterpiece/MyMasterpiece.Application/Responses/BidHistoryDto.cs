using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class BidHistoryDto
    {
        public string ItemName { get; set; }
        public decimal LastBid { get; set; }
        public decimal OpeningBid { get; set; }
        public DateTime EndTime { get; set; }
        public int ItemId { get; set; }
    }
}
