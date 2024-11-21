using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.Interfaces;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;
        public PaymentController(ILogger<PaymentController> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;

        }
        [HttpPost("CreatePaymentIntent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] MyMasterpiece.Application.DTOs.PaymentRequest paymentRequestDto)
        {
            _logger.LogInformation("Received request to create payment intent for auction ID {AuctionId}", paymentRequestDto.AuctionId);

            try
            {
                if (paymentRequestDto == null || paymentRequestDto.Amount <= 0 || paymentRequestDto.AuctionId <= 0)
                {
                    return BadRequest(new { message = "Invalid payment request." });
                }

                var clientSecret = await _paymentService.CreatePaymentIntentAsync(paymentRequestDto);

                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for auction ID {AuctionId}", paymentRequestDto.AuctionId);
                return StatusCode(500, new { message = "Error creating payment intent.", details = ex.Message });
            }
        }
        [HttpGet("GetAuctionDetailsByPayment/{paymentId}")]
        public async Task<IActionResult> GetAuctionDetailsByPayment(int paymentId)
        {
            try
            {
                var auctionDetails = await _paymentService.GetAuctionDetailsByPaymentAsync(paymentId);

                if (auctionDetails == null)
                {
                    return NotFound(new { message = "Payment not found." });
                }

                return Ok(auctionDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving auction details for payment ID {PaymentId}", paymentId);
                return StatusCode(500, new { message = "Error retrieving auction details by payment.", details = ex.Message });
            }
        }
        [HttpPut("UpdatePaymentStatus/{paymentId}")]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, [FromBody] PaymentStatusUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Payment status is required." });
            }

            try
            {
                var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, request.Status);

                if (!result)
                {
                    return NotFound(new { message = "Payment not found." });
                }

                return Ok(new { message = "Payment status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for Payment ID {PaymentId}", paymentId);
                return StatusCode(500, new { message = "Error updating payment status.", details = ex.Message });
            }
        }
        [HttpPost("CreateOrderHistory/{paymentId}")]
        public async Task<IActionResult> CreateOrderHistory(int paymentId)
        {
            var result = await _paymentService.CreateOrderHistoryAsync(paymentId);

            if (!result)
            {
                return NotFound(new { message = "No payment record found for this payment ID." });
            }

            return Ok(new { message = "Order history created successfully." });
        }

        [HttpGet("GetThankYouDetailsByPayment/{paymentId}")]
        public async Task<IActionResult> GetThankYouDetailsByPayment(int paymentId)
        {
            var details = await _paymentService.GetThankYouDetailsByPaymentAsync(paymentId);

            if (details == null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Ok(details);
        }
    }
}
