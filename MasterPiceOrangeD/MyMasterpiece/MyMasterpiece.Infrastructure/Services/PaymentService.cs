using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.MyMasterpiece.Application.DTOs.PaymentDto;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Domain.Entities;
using MyMasterpiece.Infrastructure.Data;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyMasterpiece.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly AuctionDbContext _context;

        public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger, AuctionDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<string> CreatePaymentIntentAsync(MyMasterpiece.Application.DTOs.PaymentRequest paymentRequestDto)
        {
            _logger.LogInformation("Creating payment intent for auction ID {AuctionId}", paymentRequestDto.AuctionId);

            try
            {

                StripeConfiguration.ApiKey = _configuration["Stripe:ApiKey"];


                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(paymentRequestDto.Amount * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                {
                    { "auctionId", paymentRequestDto.AuctionId.ToString() }
                }
                };


                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation("Payment intent created successfully for auction ID {AuctionId}", paymentRequestDto.AuctionId);

                return paymentIntent.ClientSecret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment intent for auction ID {AuctionId}", paymentRequestDto.AuctionId);
                throw new InvalidOperationException("Failed to create payment intent.", ex);
            }
        }
        public async Task<MyMasterpiece.Application.DTOs.AuctionDetailsResponse> GetAuctionDetailsByPaymentAsync(int paymentId)
        {
            _logger.LogInformation("Fetching auction details for payment ID {PaymentId}", paymentId);

            var payment = await _context.Payments
                .Include(p => p.Auction)
                .ThenInclude(a => a.Product)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment with ID {PaymentId} not found.", paymentId);
                return null;
            }

            var auction = payment.Auction;

            var auctionDetails = new MyMasterpiece.Application.DTOs.AuctionDetailsResponse
            {
                AuctionId = auction.AuctionId,
                ProductName = auction.Product.ProductName,
                CurrentHighestBid = auction.CurrentHighestBid,
                TotalAmount = auction.CurrentHighestBid,
                image = auction.Product.ImageUrl
            };

            _logger.LogInformation("Auction details retrieved successfully for payment ID {PaymentId}", paymentId);

            return auctionDetails;
        }
        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            _logger.LogInformation("Updating payment status for Payment ID {PaymentId} to {Status}", paymentId, status);

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment with ID {PaymentId} not found.", paymentId);
                return false;
            }

            payment.PaymentStatus = status;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Payment status for Payment ID {PaymentId} updated to {Status}", paymentId, status);

            return true;
        }

        public async Task<bool> CreateOrderHistoryAsync(int paymentId)
        {
            _logger.LogInformation("Attempting to create order history for payment ID {PaymentId}", paymentId);


            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
            {
                _logger.LogWarning("No payment record found for payment ID {PaymentId}", paymentId);
                return false;
            }


            var orderHistory = new OrderHistory
            {
                AuctionId = payment.AuctionId,
                UserId = payment.UserId,
                TotalAmount = payment.PaymentAmount,
                OrderDate = DateTime.Now
            };

            _context.OrderHistories.Add(orderHistory);


            await _context.SaveChangesAsync();

            _logger.LogInformation("Order history created successfully for payment ID {PaymentId}", paymentId);
            return true;
        }
        public async Task<ThankYouDetailsDto> GetThankYouDetailsByPaymentAsync(int paymentId)
        {
            _logger.LogInformation("Fetching thank you details for payment ID {PaymentId}", paymentId);

            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Auction)
                        .ThenInclude(a => a.Product)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found.", paymentId);
                    return null;
                }

                if (payment.Auction == null || payment.Auction.Product == null)
                {
                    _logger.LogWarning("Auction or product details are missing for payment ID {PaymentId}.", paymentId);
                    return null;
                }

                var auction = payment.Auction;
                auction.AuctionStatus = "Completed";
                _context.Auctions.Update(auction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Auction status updated to 'Completed' for auction ID {AuctionId}.", auction.AuctionId);

                return new ThankYouDetailsDto
                {
                    ProductName = auction.Product.ProductName,
                    ImageUrl = auction.Product.ImageUrl,
                    DeliveryDate = DateTime.UtcNow.AddDays(3).ToString("dd MMM yyyy"),
                    DeliveryAddress = payment.User.Address,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching thank you details for payment ID {PaymentId}.", paymentId);
                throw new InvalidOperationException("Failed to fetch thank you details. Please try again later.", ex);
            }
        }


    }

}
