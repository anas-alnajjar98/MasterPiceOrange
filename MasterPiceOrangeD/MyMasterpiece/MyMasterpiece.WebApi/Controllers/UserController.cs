using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.Interfaces;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("GetUserByID/{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found.", id);
                return NotFound(new { Message = "User not found" });
            }

            _logger.LogInformation("User with ID {Id} retrieved successfully.", id);
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto request)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", request.Email);

            var result = await _userService.RegisterUserAsync(request);

            if (result == "User with this email already exists.")
            {
                _logger.LogWarning("Registration failed for email: {Email} - User already exists.", request.Email);
                return BadRequest(new { message = result });
            }

            _logger.LogInformation("User registered successfully for email: {Email}", request.Email);
            return Ok(new { message = result });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            var result = await _userService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for email: {Email}", loginDto.Email);
                return Unauthorized(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);

            return Ok(new
            {
                UserId = result.User.UserId,
                Username = result.User.Username,
                Email = result.User.Email,
                IsAdmin = result.User.IsAdmin,
                Token = result.Token
            });
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            _logger.LogInformation("ForgotPassword endpoint called for email: {Email}", passwordDto.Email);

            
            var isSuccess = await _userService.ForgotPasswordAsync(passwordDto);

            if (isSuccess)
            {
                _logger.LogInformation("Forgot password process completed successfully for email: {Email}", passwordDto.Email);
                return Ok(new { message = "OTP sent successfully. Please check your email." });
            }
            else
            {
                _logger.LogWarning("Forgot password process failed for email: {Email}", passwordDto.Email);
                return NotFound(new { message = "User with this email does not exist or the operation failed." });
            }
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] RessetPasswordDto resetPasswordDto)
        {
            _logger.LogInformation("Received password reset request for OTP: {Otp}", resetPasswordDto.Otp);

            var isResetSuccessful = await _userService.ResetPasswordAsync(resetPasswordDto);
            if (!isResetSuccessful)
            {
                _logger.LogWarning("Password reset failed for OTP: {Otp}", resetPasswordDto.Otp);
                return BadRequest("OTP does not match or user not found.");
            }

            _logger.LogInformation("Password reset successful for OTP: {Otp}", resetPasswordDto.Otp);
            return Ok(new { message = "Password reset successfully." });
        }
        [HttpPost("GoogleSignUp")]
        public async Task<IActionResult> GoogleSignUp([FromBody] GoogleSignUpDto googleSignUpDto)
        {
            if (googleSignUpDto == null || string.IsNullOrEmpty(googleSignUpDto.Email))
            {
                _logger.LogWarning("GoogleSignUp failed due to invalid user data.");
                return BadRequest(new { message = "Invalid user data." });
            }

            var result = await _userService.GoogleSignUpAsync(googleSignUpDto);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("GoogleSignUp failed: {Reason}", result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("GoogleSignUp successful for user: {Email}", googleSignUpDto.Email);
            return Ok(new
            {
                userId = result.User.UserId,
                username = result.User.Username,
                email = result.User.Email,
                imageUrl = result.User.ImageUrl,
                token = result.Token,
                message = "User registered successfully."
            });
        }
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitContact(ContactDto contactDto)
        {
            _logger.LogInformation("SubmitContact endpoint called for UserId: {UserId}", contactDto.UserId);

            try
            {
                var result = await _userService.SubmitContactAsync(contactDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Contact submission successful for UserId: {UserId}", contactDto.UserId);
                    return Ok(new { message = result.Message });
                }

                _logger.LogWarning("Contact submission failed for UserId: {UserId}. Reason: {Message}", contactDto.UserId, result.Message);
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during contact submission for UserId: {UserId}", contactDto.UserId);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
        [HttpPost("UpdateUserInfoWithImage/{id:int}")]
        public async Task<IActionResult> UpdateUserInfoWithImage(int id, [FromForm] EditUserInfoDto edit)
        {
            _logger.LogInformation("UpdateUserInfoWithImage called for UserId: {UserId}", id);
            var result = await _userService.UpdateUserAsync(id, edit);

            if (!result)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(new { Message = "User info updated successfully" });
        }
        [HttpGet("GetUserNotification/{id:int}")]
        public async Task<IActionResult> GetUserNotification(int id)
        {
            _logger.LogInformation("API called: GetUserNotification for user ID: {UserId}", id);

            try
            {
                var notifications = await _userService.GetUserNotificationsAsync(id);

                if (notifications == null || !notifications.Any())
                {
                    _logger.LogWarning("No notifications found for user ID: {UserId}", id);
                    return NotFound(new { message = "No notifications for you." });
                }

                _logger.LogInformation("Returning {Count} notifications for user ID: {UserId}", notifications.Count, id);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving notifications for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching notifications." });
            }
        }
        [HttpGet("GetUserDashboard")]
        public async Task<IActionResult> GetUserDashboard(int userId)
        {
            _logger.LogInformation("Fetching dashboard data for user ID: {UserId}", userId);

            try
            {
                var dashboardData = await _userService.GetUserDashboardAsync(userId);

                if (dashboardData == null)
                {
                    _logger.LogWarning("No dashboard data found for user ID: {UserId}", userId);
                    return NotFound(new { message = "No dashboard data found." });
                }

                _logger.LogInformation("Successfully fetched dashboard data for user ID: {UserId}", userId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching dashboard data for user ID: {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while fetching dashboard data." });
            }
        }
        [HttpPost("EditUserResetPassword")]
        public async Task<IActionResult> EditUserResetPassword([FromBody] EditUserResetPasswordDto resetDto)
        {
            _logger.LogInformation("EditUserResetPassword endpoint called for UserID: {UserId}", resetDto.UserID);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UserID: {UserId}", resetDto.UserID);
                return BadRequest(new { message = "Invalid request data." });
            }

            try
            {
                var result = await _userService.EditUserResetPasswordAsync(resetDto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Password reset failed for UserID: {UserId}. Reason: {Reason}", resetDto.UserID, result.ErrorMessage);
                    return BadRequest(new { message = result.ErrorMessage });
                }

                _logger.LogInformation("Password reset successfully for UserID: {UserId}", resetDto.UserID);
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting password for UserID: {UserId}", resetDto.UserID);
                return StatusCode(500, new { message = "An error occurred while resetting the password. Please try again later." });
            }
        }

    }
}
