using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IBusiness
    {
        Task<MyMasterpiece.Application.DTOs.Business.navDto> Business();
    }
}
