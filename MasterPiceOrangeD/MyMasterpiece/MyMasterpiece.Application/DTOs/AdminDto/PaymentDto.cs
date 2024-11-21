﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs.AdminDto
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
    }

}
