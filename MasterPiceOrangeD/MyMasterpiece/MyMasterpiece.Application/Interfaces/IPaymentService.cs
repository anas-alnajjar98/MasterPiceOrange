using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.DTOs.MyMasterpiece.Application.DTOs.PaymentDto;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntentAsync(MyMasterpiece.Application.DTOs.PaymentRequest paymentRequestDto);
        Task<MyMasterpiece.Application.DTOs.AuctionDetailsResponse> GetAuctionDetailsByPaymentAsync(int paymentId);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
        Task<bool> CreateOrderHistoryAsync(int paymentId);
        Task<ThankYouDetailsDto> GetThankYouDetailsByPaymentAsync(int paymentId);



    }

}
