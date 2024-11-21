using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Infrastructure.Services;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly ILogger<BidController> _logger;
        private readonly IBids _bids;
        public BidController(IBids bids, ILogger<BidController> logger)
        {
            _logger = logger;
            _bids = bids;
        }
        [HttpGet("UserWinningBids/{userId:int}")]
        public async Task<IActionResult> UserWinningBids(int userId)
        {
            _logger.LogInformation("Fetching winning bids for user with ID {UserId}", userId);

            try
            {
                var winningBids = await _bids.GetUserWinningBidsAsync(userId);

                if (winningBids == null || !winningBids.Any())
                {
                    _logger.LogWarning("No winning bids found for user with ID {UserId}", userId);
                    return NotFound(new { message = "No winning bids found." });
                }

                _logger.LogInformation("Successfully fetched winning bids for user with ID {UserId}", userId);
                return Ok(winningBids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching winning bids for user with ID {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while fetching winning bids. Please try again later." });
            }
        }
    }
}
