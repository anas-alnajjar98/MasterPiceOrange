using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class EditUserInfoDto
    {
        public string? Username { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public IFormFile? Image { get; set; }
    }
}
