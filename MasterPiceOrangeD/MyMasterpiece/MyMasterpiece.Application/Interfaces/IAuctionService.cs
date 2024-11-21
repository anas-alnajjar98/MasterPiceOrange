using MyMasterpiece.Application.DTOs.AuctionDto;
using MyMasterpiece.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IAuctionService
    {
        Task<int> CreateAuctionAsync(MyMasterpiece.Application.DTOs.AuctionDto.CreateAuctionDto auctionDto);
        Task EndAuctionAsync(int auctionId);
        Task<List<MyMasterpiece.Application.DTOs.AuctionDto.PendingPaymentDto>> CheckPendingPaymentsAsync();

    }

}
