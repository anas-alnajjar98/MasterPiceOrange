using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class ContactDto
    {
        public string Name { get; set; }              // Name of the person contacting
        public string Email { get; set; }             // Email of the person contacting
        public string Subject { get; set; }           // Subject of the message
        public string Message { get; set; }           // Content of the message
        public int? UserId { get; set; }
    }
}
