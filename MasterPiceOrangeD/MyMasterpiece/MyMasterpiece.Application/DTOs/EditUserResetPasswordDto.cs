using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.DTOs
{
    public class EditUserResetPasswordDto
    {
        public int UserID { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
