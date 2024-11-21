using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Application.Responses;
using MyMasterpiece.Domain.Entities;
using MyMasterpiece.Infrastructure.Data;
using MyMasterpiece.Infrastructure.helper;

namespace MyMasterpiece.Infrastructure.Services
{
    public class UserService: IUserService
    {
        private readonly AuctionDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly TokenGenerator _tokenGenerator;
        private readonly EmailHelper _emailHelper;
        private readonly IImageService _imageService;


        public UserService(AuctionDbContext context, ILogger<UserService> logger, TokenGenerator tokenGenerator, EmailHelper emailHelper, IImageService imageService)
        {
            _context = context;
             _logger = logger;
            _tokenGenerator = tokenGenerator;
            _emailHelper = emailHelper;
            _imageService = imageService;
        }
        public async Task<UserDto> GetUserById(int id)
        {
            try
            {

                _logger.LogInformation($"Attempting to retrieve user with ID {id}");

                var user = await _context.Users
                                         .AsNoTracking()
                                         .Where(u => u.UserId == id)
                                         .Select(u => new UserDto
                                         {
                                             UserId = u.UserId,
                                             Username = u.Username,
                                             Email = u.Email,
                                             ImageUrl = u.ImageUrl
                                         })
                                         .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found.");
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching user with ID {id}");
                throw;
            }
        }
        public async Task<string> RegisterUserAsync(UserRegistrationDto request)
        {
            _logger.LogInformation("User registration attempt for email: {Email}", request.Email);

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists.", request.Email);
                return "User with this email already exists.";
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Gender = string.Empty,
                ImageUrl = "default.png",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            PasswordHelper.CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Password = request.Password;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with email: {Email}", request.Email);
            return "User registered successfully.";
        }
        public async Task<LoginResult> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && !u.IsDeleted);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email: {Email}", loginDto.Email);
                return new LoginResult { IsSuccess = false, ErrorMessage = "User not found." };
            }

            if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Login failed: Invalid password for email: {Email}", loginDto.Email);
                return new LoginResult { IsSuccess = false, ErrorMessage = "Invalid password." };
            }

            var token = _tokenGenerator.GenerateToken(user.Username, user.IsAdmin, user.UserId);

            return new LoginResult
            {
                IsSuccess = true,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                },
                Token = token
            };
        }
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto passwordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == passwordDto.Email);

            if (user != null)
            {

                Random rand = new Random();
                string otp = rand.Next(100000, 1000000).ToString();

                try
                {
                    var subject = "Password Reset Request";
                    var messageText = $@"
<html>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f9; color: #333; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
            <h2 style='color: #4CAF50; text-align: center;'>Hello, {user.Username}</h2>
            <p style='font-size: 16px; line-height: 1.6;'>
                We received a request to reset your password. Your OTP code is provided below:
            </p>
            <div style='text-align: center; margin: 20px 0;'>
                <p style='font-size: 24px; font-weight: bold; color: #4CAF50;'>{otp}</p>
                <p style='font-size: 14px; color: #888;'>This code is valid for a short period of time.</p>
            </div>
            <p style='font-size: 16px; line-height: 1.6;'>
                If you didn’t request a password reset, please ignore this email. If you have any questions or need additional assistance, please don’t hesitate to contact our support team.
            </p>
            <p style='font-size: 16px; line-height: 1.6;'>
                Best regards,<br>
                <strong>Support Team</strong>
            </p>
            <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; font-size: 12px; color: #888;'>
                © {DateTime.Now.Year} Your Company. All rights reserved.
            </div>
        </div>
    </body>
</html>";


                    _emailHelper.SendMessage(user.Username, user.Email, subject, messageText);


                    user.otp = otp;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "Failed to send forgot password email for user: {Email}", user.Email);
                    return false;
                }
            }

            return false;
        }
        public async Task<bool> ResetPasswordAsync(RessetPasswordDto resetPasswordDto)
        {
            _logger.LogInformation("Attempting to reset password for OTP: {Otp}", resetPasswordDto.Otp);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.otp == resetPasswordDto.Otp);
            if (user == null)
            {
                _logger.LogWarning("Password reset failed: OTP {Otp} not found", resetPasswordDto.Otp);
                return false;
            }

            PasswordHelper.CreatePasswordHash(resetPasswordDto.Password, out string passwordHash, out string passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Password = resetPasswordDto.Password;
            user.UpdatedAt = DateTime.Now;
            user.otp = null;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Password reset successfully for user ID: {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user ID: {UserId}", user.UserId);
                throw;
            }

            return true;
        }
        public async Task<LoginResult> GoogleSignUpAsync(GoogleSignUpDto googleSignUpDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == googleSignUpDto.Email))
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    ErrorMessage = "User with this email already exists."
                };
            }

           
            var user = new User
            {
                Username = googleSignUpDto.Username ?? "Unknown",
                Email = googleSignUpDto.Email,
                Gender = "",
                ImageUrl = googleSignUpDto.ImageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            PasswordHelper.CreatePasswordHash(googleSignUpDto.Password, out string passwordHash, out string passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Password = googleSignUpDto.Password;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            
            var token = _tokenGenerator.GenerateToken(user.Username,user.IsAdmin,user.UserId);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                ImageUrl = user.ImageUrl
            };

            return new LoginResult
            {
                IsSuccess = true,
                User = userDto,
                Token = token,
                ErrorMessage = null
            };
        }
        public async Task<ContactResponse> SubmitContactAsync(ContactDto contactDto)
        {
            var user = await _context.Users.FindAsync(contactDto.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for ID {UserId}", contactDto.UserId);
                return new ContactResponse { IsSuccess = false, Message = "You need to be logged in before submitting any comment." };
            }

            if (string.IsNullOrWhiteSpace(contactDto.Name) || string.IsNullOrWhiteSpace(contactDto.Email) || string.IsNullOrWhiteSpace(contactDto.Message))
            {
                return new ContactResponse { IsSuccess = false, Message = "Name, Email, and Message are required fields." };
            }

            var contact = new Contact
            {
                UserId = contactDto.UserId,
                Name = contactDto.Name,
                Subject = contactDto.Subject,
                Email = contactDto.Email,
                SubmittedAt = DateTime.Now,
                Message = contactDto.Message,
            };

            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();

            try
            {
                _emailHelper.SendMessage(
                    "techlearnhub.contact@gmail.com",
                    contact.Email,
                    contact.Subject ?? "New Contact Message",
                    contact.Message
                );

                _logger.LogInformation("Contact message saved and email sent for user: {UserId}", contactDto.UserId);
                return new ContactResponse { IsSuccess = true, Message = "Your contact message was submitted successfully!" };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while sending email: {Error}", ex.Message);
                return new ContactResponse { IsSuccess = false, Message = "Error occurred while sending the email." };
            }
        }
        public async Task<bool> UpdateUserAsync(int id, EditUserInfoDto editUserInfoDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return false;
            }

            if (editUserInfoDto.Image != null)
            {
                var imageUrl = await _imageService.SaveImageAsync(editUserInfoDto.Image);
                user.ImageUrl = imageUrl;
            }

            user.Username = editUserInfoDto.Username ?? user.Username;
            user.Address = editUserInfoDto.Address ?? user.Address;
            user.Gender = editUserInfoDto.Gender ?? user.Gender;

             _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User info updated successfully for UserId: {UserId}", user.UserId);
            return true;
        }
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            _logger.LogInformation("Fetching notifications for user ID: {UserId}", userId);

            try
            {
                var notifications = await _context.Notifications
                    .Where(x => x.UserId == userId)
                    .OrderBy(x => x.IsRead)
                    .ThenByDescending(x => x.CreatedAt)
                    .Select(n => new NotificationDto
                    {
                        NotificationId = n.NotificationId,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt
                    })
                    .ToListAsync();

                if (!notifications.Any())
                {
                    _logger.LogWarning("No notifications found for user ID: {UserId}", userId);
                }

                _logger.LogInformation("Fetched {Count} notifications for user ID: {UserId}", notifications.Count, userId);

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching notifications for user ID: {UserId}", userId);
                throw;
            }
        }
        public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
        {
            _logger.LogInformation("Fetching user dashboard data for user ID: {UserId}", userId);

            var userBids = await _context.Bids
                .Where(bid => bid.UserId == userId)
                .Select(bid => new BidHistoryDto
                {
                    ItemName = bid.Auction.Product.ProductName,
                    LastBid = bid.BidAmount,
                    OpeningBid = bid.Auction.Product.StartingPrice,
                    EndTime = bid.Auction.EndTime,
                    ItemId = bid.Auction.ProductId
                })
                .ToListAsync();

            var activeBids = userBids.Count;
            var winningBids = await _context.Auctions
                .Where(a => a.CurrentHighestBidderId == userId)
                .CountAsync();
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .CountAsync();

            _logger.LogInformation("Successfully fetched dashboard data for user ID: {UserId}", userId);

            return new UserDashboardDto
            {
                ActiveBids = activeBids,
                WinningBids = winningBids,
                Favorites = favorites,
                BidHistory = userBids
            };
        }
        public async Task<ServiceResult> EditUserResetPasswordAsync(EditUserResetPasswordDto resetDto)
        {
            _logger.LogInformation("Attempting password reset for UserID: {UserId}", resetDto.UserID);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == resetDto.UserID && u.Password == resetDto.Password);

            if (user == null)
            {
                _logger.LogWarning("No user found with matching ID and password for UserID: {UserId}", resetDto.UserID);
                return ServiceResult.Failure("Password not match.");
            }

            PasswordHelper.CreatePasswordHash(resetDto.ConfirmPassword, out string passwordHash, out string passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Password = resetDto.ConfirmPassword;
            user.UpdatedAt = DateTime.Now;
            user.otp = null;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Password successfully reset for UserID: {UserId}", resetDto.UserID);
                return ServiceResult.Success("Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for UserID: {UserId}", resetDto.UserID);
                return ServiceResult.Failure("An error occurred while resetting the password.");
            }
        }

    }

}
