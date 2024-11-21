using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AdminDto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal StartingPrice { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public int Quantity { get; set; }
        public string CategoryName { get; set; }
    }

}
