using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; } 
        public string Username { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; }
        public string PasswordHash { get; set; } 
        public string PasswordSalt { get; set; } 
        public bool IsAdmin { get; set; } = false;  
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        
        public string? Address { get; set; }          
        public string Gender { get; set; }          
        public string ImageUrl { get; set; } 
        public string? otp { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<Blog> Blogs { get; set; }       
        public ICollection<Bid> Bids { get; set; }         
        public ICollection<Payment> Payments { get; set; }  
    }
}
