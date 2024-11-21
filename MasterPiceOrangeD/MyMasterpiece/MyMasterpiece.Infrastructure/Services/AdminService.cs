using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.AdminDto;
using MyMasterpiece.Application.DTOs.BlogDtos;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Domain.Entities;
using MyMasterpiece.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class AdminService:IAdminService
    {
        
        private readonly AuctionDbContext _context;
        private readonly ILogger<AdminService> _logger;
        private readonly EmailHelper _emailHelper;
        private readonly IImageService _imageService;
        public AdminService(AuctionDbContext context, ILogger<AdminService> logger, EmailHelper emailHelper, IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _emailHelper = emailHelper;
            _imageService = imageService;
        }
        public async Task<int> CreateBlogAsync(CreateBlogDto createBlogDto)
        {
            _logger.LogInformation("Creating a new blog by user with ID {UserId}.", createBlogDto.AuthorId);

            try
            {
                // Use ImageService to save the image
                string imageUrl = null;
                if (createBlogDto.Image != null && createBlogDto.Image.Length > 0)
                {
                    imageUrl = await _imageService.SaveImageAsync(createBlogDto.Image);
                }

                var blog = new Blog
                {
                    Title = createBlogDto.Title,
                    Content = createBlogDto.Content,
                    ImageUrl = imageUrl,
                    PublishedAt = DateTime.UtcNow,
                    ViewCount = 0,
                    ApprovalStatus = "Accepted",
                    UserId = createBlogDto.AuthorId
                };

                await _context.Blogs.AddAsync(blog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog created successfully with ID {BlogId}.", blog.BlogId);

                return blog.BlogId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new blog.");
                throw new InvalidOperationException("Failed to create blog. Please try again later.", ex);
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Fetching all users.");

            var users = await _context.Users
                .Include(u => u.Blogs)
                .Include(u => u.Bids)
                .Include(u => u.Payments)
                .Where(u => u.IsDeleted == false)
                .ToListAsync();

            if (!users.Any())
            {
                _logger.LogWarning("No users found.");
                return new List<UserDto>();
            }

            _logger.LogInformation("Successfully fetched {Count} users.", users.Count);
            
            return users.Select(user => new MyMasterpiece.Application.DTOs.AdminDto.UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Address = user.Address,
                Gender = user.Gender,
                ImageUrl = user.ImageUrl,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Blogs = user.Blogs.Select(b => new MyMasterpiece.Application.DTOs.AdminDto.BlogDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    Content = b.Content,
                    PublishedAt = b.PublishedAt
                }).ToList(),
                Bids = user.Bids.Select(b => new MyMasterpiece.Application.DTOs.AdminDto.BidDto
                {
                    BidId = b.BidId,
                    AuctionId = b.AuctionId,
                    BidAmount = b.BidAmount,
                    BidTime = b.BidTime
                }).ToList(),
                Payments = user.Payments.Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    PaymentAmount = p.PaymentAmount,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    PaymentDueDate = p.PaymentDueDate
                }).ToList()
            }).ToList();
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            _logger.LogInformation("Attempting to delete user with ID {UserId}", userId);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return false;
            }

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} marked as deleted.", userId);
            return true;
        }
        public async Task<bool> RejectProductAsync(int productId)
        {
            _logger.LogInformation("Attempting to reject product with ID {ProductId}", productId);

            
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                return false;
            }

            
            product.ApprovalStatus = "Rejected";
            product.UpdatedAt = DateTime.Now;

            
            try
            {
                var user = await _context.Users.FindAsync(product.SellerId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {SellerId} not found for product {ProductId}.", product.SellerId, productId);
                }
                else
                {
                    _logger.LogInformation("Sending rejection email to user with ID {SellerId}.", product.SellerId);

                    string subject = "Product Rejection Notice";
                    string message = $@"
<div style='font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
    <h1 style='color: #d9534f; text-align: center;'>Product Rejection Notice</h1>
    <p style='font-size: 16px; line-height: 1.6;'>
        Dear <strong>{user.Username}</strong>,
    </p>
    <p style='font-size: 16px; line-height: 1.6;'>
        We regret to inform you that your product submission has been <strong>rejected</strong>. Unfortunately, your product does not meet the necessary requirements for approval.
    </p>
    <p style='font-size: 16px; line-height: 1.6;'>
        If you believe this is a mistake or have any questions, please do not hesitate to contact our support team for further assistance.
    </p>
    <p style='font-size: 16px; line-height: 1.6;'>
        You can reach us at <a href='mailto:support@laqta.com' style='color: #337ab7;'>support@laqta.com</a>.
    </p>
    <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;' />
    <p style='font-size: 14px; color: #888; text-align: center;'>
        Thank you for understanding.<br>
        <strong>Laqta Team</strong>
    </p>
</div>";
                    ;

                    _emailHelper.SendMessage(user.Username, user.Email, subject, message);

                    _logger.LogInformation("Rejection email sent successfully to user with ID {SellerId}.", product.SellerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the rejection email for product ID {ProductId}.", productId);
            }

            
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product with ID {ProductId} rejected successfully.", productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the rejection status for product ID {ProductId}.", productId);
                throw new InvalidOperationException($"Failed to reject product with ID {productId}.", ex);
            }
        }
        public async Task<List<ProductDto>> GetPendingProductsForAuctionAsync()
        {
            _logger.LogInformation("Fetching products with pending approval for auction placement.");

            try
            {
                var products = await _context.Products
                    .Where(a => a.ApprovalStatus == "Pending")
                    .Select(p => new MyMasterpiece.Application.DTOs.AdminDto.ProductDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        StartingPrice = p.StartingPrice,
                        ImageUrl = p.ImageUrl,
                        Stock = p.Stock,
                        Condition = p.Condition,
                        Location = p.Location,
                        Country = p.Country,
                        Brand = p.Brand,
                        Quantity = p.Quantity,
                        CategoryName = p.Category.CategoryName
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully fetched {Count} pending products for auction.", products.Count);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching pending products for auction.");
                throw new InvalidOperationException("Unable to fetch pending products. Please try again later.", ex);
            }
        }
        public async Task<List<EndedAuctionDto>> GetEndedAuctionsAsync()
        {
            _logger.LogInformation("Fetching ended auctions with pending payments.");

            try
            {
                var endedAuctions = await _context.Auctions
                    .Where(x => x.EndTime < DateTime.Now && x.IsNotificationSent == false && x.AuctionStatus == "pending_payment")
                    .Include(x => x.Product)
                    .Include(x => x.CurrentHighestBidder)
                    .Select(x => new EndedAuctionDto
                    {
                        AuctionId = x.AuctionId,
                        ProductName = x.Product.ProductName ?? "Unknown Product",
                        CurrentHighestBid = x.CurrentHighestBid,
                        HighestBidder = x.CurrentHighestBidder != null ? new BidderDto { UserId = x.CurrentHighestBidder.UserId, Username = x.CurrentHighestBidder.Username, Email = x.CurrentHighestBidder.Email } : null
                    })
                    .ToListAsync();

                if (endedAuctions == null || !endedAuctions.Any())
                {
                    _logger.LogWarning("No ended auctions with pending payments found.");
                }
                else
                {
                    _logger.LogInformation("{Count} ended auctions fetched successfully.", endedAuctions.Count);
                }

                return endedAuctions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching ended auctions.");
                throw new InvalidOperationException("Unable to fetch ended auctions. Please try again later.", ex);
            }
        }



    }
}
