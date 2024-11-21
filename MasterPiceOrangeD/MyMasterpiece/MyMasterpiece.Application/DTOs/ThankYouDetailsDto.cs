using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    namespace MyMasterpiece.Application.DTOs.PaymentDto
    {
        public class ThankYouDetailsDto
        {
            public string ProductName { get; set; }
            public string ImageUrl { get; set; }
            public string DeliveryDate { get; set; }
            public string DeliveryAddress { get; set; }
        }
    }

}
