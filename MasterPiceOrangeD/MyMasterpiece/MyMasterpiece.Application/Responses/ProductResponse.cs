using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class ProductResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? ProductId { get; set; }
    }
}
