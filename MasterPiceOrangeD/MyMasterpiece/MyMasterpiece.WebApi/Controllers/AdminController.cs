using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.DTOs.BlogDtos;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Infrastructure.Services;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _logger = logger;
            _adminService = adminService;
        }
        [HttpPost("CreateBlog")]
        public async Task<IActionResult> CreateBlog([FromForm] CreateBlogDto createBlogDto)
        {
            _logger.LogInformation("API call: CreateBlog initiated.");

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid blog data." });
            }

            try
            {
                var blogId = await _adminService.CreateBlogAsync(createBlogDto);

                return Ok(new { message = "Blog created successfully.", blogId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error occurred in CreateBlog API.");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("API call: GetAllUsers");

            try
            {
                var users = await _adminService.GetAllUsersAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("No users found.");
                    return NotFound(new { message = "No users found." });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                return StatusCode(500, new { message = "An error occurred while fetching users." });
            }
        }
        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            _logger.LogInformation("API call: DeleteUser with ID {UserId}", userId);

            try
            {
                var result = await _adminService.DeleteUserAsync(userId);

                if (!result)
                {
                    _logger.LogWarning("User with ID {UserId} not found during deletion attempt.", userId);
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "User marked as deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user with ID {UserId}.", userId);
                return StatusCode(500, new { message = "An error occurred while deleting the user." });
            }
        }
        [HttpPut("RejectProduct/{productId}")]
        public async Task<IActionResult> RejectProduct(int productId)
        {
            _logger.LogInformation("API call: RejectProduct with ID {ProductId}", productId);

            try
            {
                var result = await _adminService.RejectProductAsync(productId);

                if (!result)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found during rejection.", productId);
                    return NotFound(new { message = "Product not found." });
                }

                return Ok(new { message = "Product rejected successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting the product with ID {ProductId}.", productId);
                return StatusCode(500, new { message = "Error rejecting product.", details = ex.Message });
            }
        }
        [HttpGet("GetAllProductToPlaceAuction")]
        public async Task<IActionResult> GetAllProductToPlaceAuction()
        {
            try
            {
                var products = await _adminService.GetPendingProductsForAuctionAsync();

                if (products == null || !products.Any())
                {
                    _logger.LogWarning("No pending products found for auction placement.");
                    return NotFound(new { message = "No pending products found for auction placement." });
                }

                return Ok(products);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error occurred in GetAllProductToPlaceAuction endpoint.");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in GetAllProductToPlaceAuction endpoint.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        [HttpGet("EndedAuction")]
        public async Task<IActionResult> GetEndedAuction()
        {
            try
            {
                var endedAuctions = await _adminService.GetEndedAuctionsAsync();

                if (endedAuctions == null || !endedAuctions.Any())
                {
                    return NotFound(new { message = "No auctions have ended." });
                }

                return Ok(new
                {
                    message = "Auctions fetched successfully.",
                    endedAuctions
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error occurred in GetEndedAuction endpoint.");
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in GetEndedAuction endpoint.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }


    }
}
