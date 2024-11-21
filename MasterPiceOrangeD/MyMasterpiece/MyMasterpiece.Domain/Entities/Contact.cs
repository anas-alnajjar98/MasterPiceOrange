using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Domain.Entities
{
    public class Contact
    {
        public int ContactId { get; set; }           
        public string Name { get; set; }            
        public string Email { get; set; }            
        public string Subject { get; set; }           
        public string Message { get; set; }           
        public DateTime SubmittedAt { get; set; } = DateTime.Now;  

       
        public int? UserId { get; set; }
        public User User { get; set; }
    }
}
