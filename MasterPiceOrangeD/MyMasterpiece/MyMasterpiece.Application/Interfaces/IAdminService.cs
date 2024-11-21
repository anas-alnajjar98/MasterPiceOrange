using MyMasterpiece.Application.DTOs.AdminDto;
using MyMasterpiece.Application.DTOs.BlogDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IAdminService
    {
        Task<List<MyMasterpiece.Application.DTOs.AdminDto.UserDto>> GetAllUsersAsync();

        Task<bool> DeleteUserAsync(int userId);

        Task<List<MyMasterpiece.Application.DTOs.AdminDto.ProductDto>> GetPendingProductsForAuctionAsync();
        Task<bool> RejectProductAsync(int productId);

        Task<List<MyMasterpiece.Application.DTOs.AdminDto.EndedAuctionDto>> GetEndedAuctionsAsync();
        Task<int> CreateBlogAsync(MyMasterpiece.Application.DTOs.BlogDtos.CreateBlogDto createBlogDto);

    }
}
