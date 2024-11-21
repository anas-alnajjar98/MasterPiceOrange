using MyMasterpiece.Application.DTOs.WinningBidDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IBids
    {
        Task<List<MyMasterpiece.Application.DTOs.WinningBidDto.WinningBidDto>> GetUserWinningBidsAsync(int userId);
    }
}
