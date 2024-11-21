using Microsoft.AspNetCore.Http;
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserById(int id);
        Task<string> RegisterUserAsync(UserRegistrationDto request);
        Task<LoginResult> LoginAsync(LoginDto loginDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto passwordDto);
        Task<bool> ResetPasswordAsync(RessetPasswordDto resetPasswordDto);
        Task<LoginResult> GoogleSignUpAsync(GoogleSignUpDto googleSignUpDto);
        Task<ContactResponse> SubmitContactAsync(ContactDto contactDto);
        Task<bool> UpdateUserAsync(int id, EditUserInfoDto editUserInfoDto);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task<UserDashboardDto> GetUserDashboardAsync(int userId);
        Task<ServiceResult> EditUserResetPasswordAsync(EditUserResetPasswordDto resetDto);


    }
}
