using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.Interfaces;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        [HttpPost("CreateAuction")]
        public async Task<IActionResult> CreateAuction([FromBody] MyMasterpiece.Application.DTOs.AuctionDto.CreateAuctionDto auctionDto)
        {
            try
            {
                var auctionId = await _auctionService.CreateAuctionAsync(auctionDto);
                return Ok(new { message = "Auction created and product approved", auctionId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the auction.", details = ex.Message });
            }
        }
        [HttpPost("CheckPendingPayments")]
        public async Task<IActionResult> CheckPendingPayments()
        {
            try
            {
                _logger.LogInformation("Starting to check pending payments.");
                var rescheduledPayments = await _auctionService.CheckPendingPaymentsAsync();

                if (!rescheduledPayments.Any())
                {
                    _logger.LogInformation("No pending payments found.");
                    return Ok(new { message = "No pending payments found." });
                }

                return Ok(new
                {
                    message = "Pending payments checked and necessary actions taken.",
                    rescheduledPayments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking pending payments.");
                return StatusCode(500, new { message = "An error occurred while checking pending payments.", details = ex.Message });
            }
        }
    }
}
