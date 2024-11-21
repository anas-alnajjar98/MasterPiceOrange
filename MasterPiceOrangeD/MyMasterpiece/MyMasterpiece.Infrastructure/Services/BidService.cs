using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.WinningBidDto;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class BidService:IBids
    {
        private readonly AuctionDbContext _context;
        private readonly ILogger<BidService> _logger;
        public BidService(AuctionDbContext context, ILogger<BidService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<WinningBidDto>> GetUserWinningBidsAsync(int userId)
        {
            _logger.LogInformation("Start fetching winning bids for user with ID {UserId}", userId);

            try
            {
                
                var winningBids = await _context.OrderHistories
                    .Where(oh => oh.UserId == userId)
                    .Include(oh => oh.Auction)
                    .ThenInclude(a => a.Product)
                    .Select(oh => new WinningBidDto
                    {
                        ProductName = oh.Auction.Product.ProductName,
                        Amount = oh.TotalAmount,
                        ProductQuantity = oh.Auction.Product.Quantity,
                        ProductCondition = oh.Auction.Product.Condition,
                        ProductDescription = oh.Auction.Product.Description,
                        ProductImage = oh.Auction.Product.ImageUrl,
                        OrderDate = oh.OrderDate
                    })
                    .ToListAsync();

                if (!winningBids.Any())
                {
                    _logger.LogWarning("No winning bids found for user with ID {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("Successfully fetched {Count} winning bids for user with ID {UserId}", winningBids.Count, userId);
                }

                return winningBids;
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "An error occurred while fetching winning bids for user with ID {UserId}", userId);

                throw new InvalidOperationException("Failed to fetch winning bids. Please try again later.", ex);
            }
        }

    }
}
