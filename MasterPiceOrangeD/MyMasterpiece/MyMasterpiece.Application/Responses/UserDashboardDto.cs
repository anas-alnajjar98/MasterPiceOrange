using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class UserDashboardDto
    {
        public int ActiveBids { get; set; }
        public int WinningBids { get; set; }
        public int Favorites { get; set; }
        public List<BidHistoryDto> BidHistory { get; set; }
    }
}
