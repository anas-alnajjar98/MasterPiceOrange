using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMasterpiece.Application.DTOs;
using MyMasterpiece.Application.Responses;
using MyMasterpiece.Application.Responses.ResponseForGetallproduct;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IProducts
    {
        Task<List<MyMasterpiece.Application.Responses.AuctionProductDto>> GetAuctionProductsForHomePageAsync();
        Task<MyMasterpiece.Application.Responses.AuctionProductDto> GetAuctionProductForHomePageLargeCardAsync();
        Task<MyMasterpiece.Application.Responses.ProductDetailsDto> GetProductByAuctionIdAsync(int auctionId);
        Task<BidResponseDto> PlaceBidAsync(PlaceBids placeBidDto);
        Task<List<CategoryWithProductCountDto>> GetAllCategoriesWithTotalProductsAsync();
        Task<MyMasterpiece.Application.Responses.ResponseForGetallproduct.ProductListResponse> GetProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize);
        Task<ProductResponse> CreateProductAsync(CreateProductDto productDto);
        Task<MyMasterpiece.Application.Responses.ResponseForGetallproduct.ProductListResponse> GetAllProductsAsync(int pageNumber, int pageSize);

    }
}
