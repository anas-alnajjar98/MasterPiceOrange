﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }           
        public string CategoryName { get; set; } 

        // Navigation Properties
        public ICollection<Product> Products { get; set; }
    }
}
