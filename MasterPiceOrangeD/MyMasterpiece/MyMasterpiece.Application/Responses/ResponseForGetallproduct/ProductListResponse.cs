using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses.ResponseForGetallproduct
{
    public class ProductListResponse
    {
        public int TotalAuctions { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<MyMasterpiece.Application.DTOs.DtosForGetallproduct. AuctionProductDto> Auctions { get; set; }
    }
}
