using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Application.Responses;
using MyMasterpiece.Application.Responses.ResponseForGetallproduct;
using MyMasterpiece.Domain.Entities;
using MyMasterpiece.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class ProductsService:IProducts
    {
        private readonly AuctionDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly TokenGenerator _tokenGenerator;
        private readonly EmailHelper _emailHelper;
        private readonly IImageService _imageService;
        public ProductsService(AuctionDbContext context, ILogger<UserService> logger, TokenGenerator tokenGenerator, EmailHelper emailHelper, IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _tokenGenerator = tokenGenerator;
            _emailHelper = emailHelper;
            _imageService = imageService;
        }
        public async Task<List<AuctionProductDto>> GetAuctionProductsForHomePageAsync()
        {
            try
            {
                _logger.LogInformation("Fetching auction products for homepage.");

               
                var auctionProducts = await _context.Auctions
                    .Include(a => a.Product)
                    .Include(a => a.CurrentHighestBidder)
                    .Where(a => a.Product.ApprovalStatus == "Accepted" &&
                                a.EndTime > DateTime.Now &&
                                a.AuctionStatus == "ongoing")
                    .OrderBy(a => Guid.NewGuid()) 
                    .Take(4) 
                    .Select(a => new AuctionProductDto
                    {
                        AuctionId = a.AuctionId,
                        EndTime = a.EndTime,
                        CurrentHighestBid = a.CurrentHighestBid,
                        ProductName = a.Product.ProductName,
                        Description = a.Product.Description,
                        ImageUrl = a.Product.ImageUrl,
                        StartingPrice = a.Product.StartingPrice,
                        HighestBidderName = a.CurrentHighestBidder != null ? a.CurrentHighestBidder.Username : "No bids yet"
                    })
                    .ToListAsync();

               
                if (!auctionProducts.Any())
                {
                    _logger.LogWarning("No auction products found for homepage.");
                }
                else
                {
                    _logger.LogInformation("{Count} auction products fetched successfully for homepage.", auctionProducts.Count);
                }

                return auctionProducts;
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "An error occurred while fetching auction products for homepage.");
                throw new InvalidOperationException("Failed to fetch auction products for homepage. Please try again later.", ex);
            }
        }

        public async Task<AuctionProductDto> GetAuctionProductForHomePageLargeCardAsync()
        {
            try
            {
                _logger.LogInformation("Fetching auction product for large card on the homepage.");

                var auctionProduct = await _context.Auctions
                    .Include(a => a.Product)
                    .Include(a => a.CurrentHighestBidder)
                    .Where(a => a.Product.ApprovalStatus == "Accepted" &&
                                a.EndTime > DateTime.Now &&
                                a.AuctionStatus == "ongoing")
                    .OrderBy(a => Guid.NewGuid())
                    .Take(1)
                    .Select(a => new AuctionProductDto
                    {
                        AuctionId = a.AuctionId,
                        EndTime = a.EndTime,
                        CurrentHighestBid = a.CurrentHighestBid,
                        ProductName = a.Product.ProductName,
                        Description = a.Product.Description,
                        ImageUrl = a.Product.ImageUrl,
                        StartingPrice = a.Product.StartingPrice,
                        HighestBidderName = a.CurrentHighestBidder != null ? a.CurrentHighestBidder.Username : "No bids yet"
                    })
                    .FirstOrDefaultAsync();

                if (auctionProduct == null)
                {
                    _logger.LogWarning("No auction products found.");
                    return null;
                }

                _logger.LogInformation("Successfully fetched auction product for large card.");
                return auctionProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching auction product for large card.");
                throw;
            }
        }
        public async Task<ProductDetailsDto> GetProductByAuctionIdAsync(int auctionId)
        {
            _logger.LogInformation("Fetching product details for Auction ID: {AuctionId}", auctionId);

            try
            {
                var product = await _context.Auctions
                    .Include(a => a.Product)
                    .Include(a => a.CurrentHighestBidder)
                    .Where(a => a.Product.ApprovalStatus == "Accepted" &&
                                a.AuctionStatus == "ongoing" &&
                                a.AuctionId == auctionId &&
                                a.EndTime > DateTime.Now)
                    .Select(a => new ProductDetailsDto
                    {
                        EndTime = a.EndTime,
                        CurrentHighestBid = a.CurrentHighestBid,
                        TotalBids = a.Bids.Count(),
                        ProductName = a.Product.ProductName,
                        Description = a.Product.Description,
                        ImageUrl = a.Product.ImageUrl,
                        StartingPrice = a.Product.StartingPrice,
                        Country = a.Product.Country,
                        Brand = a.Product.Brand,
                        Quantity = a.Product.Quantity,
                        Stock = a.Product.Stock,
                        Views = a.Product.Views,
                        CreatedAt = a.Product.CreatedAt,
                        HighestBidderName = a.CurrentHighestBidder != null ? a.CurrentHighestBidder.Username : "No bids yet"
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    _logger.LogWarning("No product found for Auction ID: {AuctionId}", auctionId);
                    return null;
                }

                _logger.LogInformation("Product details fetched successfully for Auction ID: {AuctionId}", auctionId);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching product details for Auction ID: {AuctionId}", auctionId);
                throw new InvalidOperationException("An error occurred while processing the request. Please try again later.", ex);
            }
        }
        public async Task<BidResponseDto> PlaceBidAsync(MyMasterpiece.Application.DTOs.PlaceBids placeBidDto)
        {
            _logger.LogInformation("Placing bid for Auction ID: {AuctionId}", placeBidDto.AuctionId);

            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .Include(a => a.CurrentHighestBidder)
                .Include(a => a.Product)
                .SingleOrDefaultAsync(a => a.AuctionId == placeBidDto.AuctionId);

            if (auction == null)
            {
                _logger.LogWarning("Auction not found for Auction ID: {AuctionId}", placeBidDto.AuctionId);
                return new BidResponseDto { IsSuccess = false, Message = "Auction not found" };
            }

            if (placeBidDto.BidAmount <= auction.CurrentHighestBid || placeBidDto.BidAmount < auction.Product.StartingPrice)
            {
                _logger.LogWarning("Bid amount {BidAmount} is not valid for Auction ID: {AuctionId}", placeBidDto.BidAmount, placeBidDto.AuctionId);
                return new BidResponseDto { IsSuccess = false, Message = "Bid must be higher than the current highest bid or the starting price." };
            }

            var newBid = new Bid
            {
                BidAmount = placeBidDto.BidAmount,
                AuctionId = auction.AuctionId,
                UserId = placeBidDto.UserId,
                BidTime = DateTime.Now
            };

            _context.Bids.Add(newBid);

            auction.CurrentHighestBid = placeBidDto.BidAmount;
            auction.CurrentHighestBidderId = newBid.UserId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bid placed successfully for Auction ID: {AuctionId}", placeBidDto.AuctionId);
                return new BidResponseDto
                {
                    IsSuccess = true,
                    Message = "Bid placed successfully",
                    AuctionId = auction.AuctionId,
                    HighestBid = auction.CurrentHighestBid,
                    HighestBidderId = auction.CurrentHighestBidderId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while placing the bid for Auction ID: {AuctionId}", placeBidDto.AuctionId);
                throw new InvalidOperationException("Failed to place the bid. Please try again later.", ex);
            }
        }

        public async Task<List<CategoryWithProductCountDto>> GetAllCategoriesWithTotalProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all categories with total product counts.");

                var categories = await _context.Categories
                    .Select(c => new CategoryWithProductCountDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        TotalProducts = c.Products.Count()
                    })
                    .ToListAsync();

                if (!categories.Any())
                {
                    _logger.LogWarning("No categories found in the database.");
                }
                else
                {
                    _logger.LogInformation("Successfully fetched {Count} categories.", categories.Count);
                }

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories.");
                throw new InvalidOperationException("Failed to fetch categories. Please try again later.", ex);
            }
        }
        public async Task<MyMasterpiece.Application.Responses.ResponseForGetallproduct.ProductListResponse> GetProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogWarning("Invalid pageNumber or pageSize. pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);
                throw new ArgumentException("Page number and page size must be greater than zero.");
            }

            _logger.LogInformation("Fetching products for category ID: {CategoryId}, PageNumber: {PageNumber}, PageSize: {PageSize}", categoryId, pageNumber, pageSize);

            try
            {
                var totalProducts = await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .CountAsync();

                var skip = (pageNumber - 1) * pageSize;

                var products = await _context.Auctions
                    .Include(p => p.Product)
                    .Where(p => p.Product.CategoryId == categoryId && p.Product.ApprovalStatus == "Accepted" && p.EndTime > DateTime.Now)
                    .Select(p => new MyMasterpiece.Application.DTOs.DtosForGetallproduct.AuctionProductDto
                    {
                        AuctionId = p.AuctionId,
                        CurrentHighestBid = p.CurrentHighestBid,
                        EndTime = p.EndTime,
                        StartTime = p.StartTime,
                        ProductDetails = new MyMasterpiece.Application.DTOs.DtosForGetallproduct.ProductDetailsDto
                        {
                            ProductName = p.Product.ProductName,
                            Description = p.Product.Description,
                            ImageUrl = p.Product.ImageUrl,
                            StartingPrice = p.Product.StartingPrice,
                            CategoryName = p.Product.Category.CategoryName
                        }
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                if (!products.Any())
                {
                    _logger.LogWarning("No products found for category ID: {CategoryId}", categoryId);
                }

                return new  MyMasterpiece.Application.Responses.ResponseForGetallproduct.ProductListResponse
                {
                    TotalAuctions = totalProducts,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                    Auctions = products
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products for category ID: {CategoryId}", categoryId);
                throw new InvalidOperationException("An error occurred while fetching products. Please try again later.", ex);
            }
        }
        public async Task<ProductResponse> CreateProductAsync(MyMasterpiece.Application.DTOs.CreateProductDto productDto)
        {
            try
            {
                _logger.LogInformation("Saving image for the product.");
                var imageUrl = await _imageService.SaveImageAsync(productDto.Image);

                _logger.LogInformation("Image saved successfully: {ImageUrl}", imageUrl);

                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    Description = productDto.Description,
                    StartingPrice = productDto.StartingPrice,
                    ImageUrl = imageUrl,
                    Stock = productDto.Stock,
                    Condition = productDto.Condition,
                    Location = productDto.Location,
                    Country = productDto.Country,
                    Brand = productDto.Brand,
                    Quantity = productDto.Quantity,
                    CategoryId = productDto.CategoryId,
                    SellerId = productDto.UserId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ApprovalStatus = "Pending"
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product created successfully with ID {ProductId}.", product.ProductId);

                return new ProductResponse
                {
                    IsSuccess = true,
                    Message = "Product created successfully",
                    ProductId = product.ProductId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the product.");
                return new ProductResponse
                {
                    IsSuccess = false,
                    Message = "An error occurred while creating the product."
                };
            }
        }
        public async Task<ProductListResponse> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching products for page {PageNumber} with page size {PageSize}.", pageNumber, pageSize);

            var totalAuctions = await _context.Auctions
                .Where(p => p.AuctionStatus == "ongoing" && p.EndTime > DateTime.Now)
                .CountAsync();

            var auctions = await _context.Auctions
                .Include(p => p.Product)
                .ThenInclude(p => p.Category)
                .Where(p => p.AuctionStatus == "ongoing" && p.EndTime > DateTime.Now)
                .OrderBy(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new MyMasterpiece.Application.DTOs.DtosForGetallproduct.AuctionProductDto
                {
                    AuctionId = p.AuctionId,
                    CurrentHighestBid = p.CurrentHighestBid,
                    EndTime = p.EndTime,
                    StartTime = p.StartTime,
                    ProductDetails = new MyMasterpiece.Application.DTOs.DtosForGetallproduct.ProductDetailsDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
                        Description = p.Product.Description,
                        ImageUrl = p.Product.ImageUrl,
                        StartingPrice = p.Product.StartingPrice,
                        CategoryName = p.Product.Category.CategoryName
                    }
                })
                .ToListAsync();

            _logger.LogInformation("Fetched {TotalAuctions} products successfully for page {PageNumber}.", totalAuctions, pageNumber);

            return new MyMasterpiece.Application.Responses.ResponseForGetallproduct.ProductListResponse
            {
                TotalAuctions = totalAuctions,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalAuctions / pageSize),
                Auctions = auctions
            };
        }


    }
}
