using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using MyMasterpiece.Application.Interfaces;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Infrastructure.Services;
using MyMasterpiece.Application.DTOs;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProducts _products;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(IProducts products, ILogger<ProductsController> logger)
        {
            _products = products;
            _logger = logger;

        }
        [HttpGet("GetAuctionProductsForHomePage")]
        public async Task<IActionResult> GetAuctionProductsForHomePage()
        {
            _logger.LogInformation("Fetching auction products for the home page.");

            try
            {
                var products = await _products.GetAuctionProductsForHomePageAsync();

                if (products == null || !products.Any())
                {
                    _logger.LogWarning("No auction products found for the home page.");
                    return NotFound(new { message = "No auction products available." });
                }

                _logger.LogInformation("Auction products successfully retrieved for the home page.");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching auction products.");
                return StatusCode(500, new { message = "An error occurred while fetching auction products. Please try again later." });
            }
        }
        [HttpGet("GetAuctionProductsForHomePageForLargeCard")]
        public async Task<IActionResult> GetAuctionProductsForHomePageForLargeCard()
        {
            _logger.LogInformation("Request received to fetch auction product for large card.");

            try
            {
                var auctionProduct = await _products.GetAuctionProductForHomePageLargeCardAsync();

                if (auctionProduct == null)
                {
                    _logger.LogWarning("No auction product found for large card.");
                    return NotFound(new { message = "No auction products found." });
                }

                _logger.LogInformation("Successfully retrieved auction product for large card.");
                return Ok(auctionProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching auction product for large card.");
                return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
            }
        }
        [HttpGet("GetProductByAuctionID/{id:int}")]
        public async Task<IActionResult> GetProductByAuctionID(int id)
        {
            _logger.LogInformation("Received request to fetch product details for Auction ID: {AuctionId}", id);

            try
            {
                var productDetails = await _products.GetProductByAuctionIdAsync(id);

                if (productDetails == null)
                {
                    _logger.LogWarning("Product not found for Auction ID: {AuctionId}", id);
                    return NotFound(new { message = "Product not found or approval status is not accepted." });
                }

                _logger.LogInformation("Product details returned successfully for Auction ID: {AuctionId}", id);
                return Ok(productDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching product details for Auction ID: {AuctionId}", id);
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }
        [HttpPost("PlaceBid")]
        public async Task<IActionResult> PlaceBid([FromBody] MyMasterpiece.Application.DTOs.PlaceBids placeBidDto)
        {
            _logger.LogInformation("Received request to place bid for Auction ID: {AuctionId}", placeBidDto.AuctionId);

            try
            {
                var response = await _products.PlaceBidAsync(placeBidDto);

                if (!response.IsSuccess)
                {
                    _logger.LogWarning("Bid placement failed: {Message}", response.Message);
                    return BadRequest(new { message = response.Message });
                }

                _logger.LogInformation("Bid placed successfully for Auction ID: {AuctionId}", response.AuctionId);
                return Ok(new
                {
                    message = response.Message,
                    AuctionId = response.AuctionId,
                    HighestBid = response.HighestBid,
                    HighestBidderId = response.HighestBidderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while placing bid for Auction ID: {AuctionId}", placeBidDto.AuctionId);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
        [HttpGet("GetALLCategoriesWithTotalProducts")]
        public async Task<IActionResult> GetAllCategoriesWithTotalProducts()
        {
            _logger.LogInformation("Received request to fetch all categories with total products.");

            try
            {
                var categories = await _products.GetAllCategoriesWithTotalProductsAsync();

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("No categories found to return.");
                    return NotFound(new { message = "No categories found." });
                }

                _logger.LogInformation("Returning {Count} categories.", categories.Count);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching categories.");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
        [HttpGet("GetProductsByCategory/{categoryId:int}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId, int pageNumber = 1, int pageSize = 9)
        {
            _logger.LogInformation("Received request to fetch products for category ID: {CategoryId}, PageNumber: {PageNumber}, PageSize: {PageSize}", categoryId, pageNumber, pageSize);

            try
            {
                var result = await _products.GetProductsByCategoryAsync(categoryId, pageNumber, pageSize);

                if (!result.Auctions.Any())
                {
                    _logger.LogWarning("No products found for category ID: {CategoryId}", categoryId);
                    return NotFound(new { message = "No products found under this category." });
                }

                _logger.LogInformation("Returning {Count} products for category ID: {CategoryId}", result.Auctions.Count, categoryId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments for fetching products.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products for category ID: {CategoryId}", categoryId);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
        [HttpPost("PostProduct")]
        public async Task<IActionResult> PostProduct([FromForm] CreateProductDto productDto)
        {
            _logger.LogInformation("Starting product creation for user ID {UserId}.", productDto.UserId);

            if (productDto.UserId == 0)
            {
                _logger.LogWarning("User ID is missing in the request.");
                return BadRequest(new { message = "User ID is required." });
            }

            if (productDto.Image == null || productDto.Image.Length == 0)
            {
                _logger.LogWarning("Image is missing in the request.");
                return BadRequest(new { message = "Image is required." });
            }

            try
            {
               
                var result = await _products.CreateProductAsync(productDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Product created successfully with ID {ProductId}.", result.ProductId);
                    return Ok(new { message = result.Message, productId = result.ProductId });
                }

                _logger.LogWarning("Product creation failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the product for user ID {UserId}.", productDto.UserId);
                return StatusCode(500, new { message = "An error occurred while creating the product.", details = ex.Message });
            }
        }
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts(int pageNumber = 1, int pageSize = 9)
        {
            _logger.LogInformation("Fetching all products for page {PageNumber} with page size {PageSize}.", pageNumber, pageSize);

            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogWarning("Invalid page number or page size provided.");
                return BadRequest(new { message = "Page number and page size must be greater than zero." });
            }

            try
            {
                var response = await _products.GetAllProductsAsync(pageNumber, pageSize);

                if (response.Auctions == null || !response.Auctions.Any())
                {
                    _logger.LogWarning("No products found.");
                    return NotFound(new { message = "No products found." });
                }

                _logger.LogInformation("{TotalAuctions} products fetched successfully for page {PageNumber}.", response.TotalAuctions, pageNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching products.");
                return StatusCode(500, new { message = "An error occurred while fetching products. Please try again later." });
            }
        }

    }
}
