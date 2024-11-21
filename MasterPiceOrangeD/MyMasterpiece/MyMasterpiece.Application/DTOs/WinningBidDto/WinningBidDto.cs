using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.WinningBidDto
{
    public class WinningBidDto
    {
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public int ProductQuantity { get; set; }
        public string ProductCondition { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
