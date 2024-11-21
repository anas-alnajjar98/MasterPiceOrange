using MyMasterpiece.Application.DTOs.BlogDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Interfaces
{
    public interface IHome
    {
        Task<MyMasterpiece.Application.DTOs.BlogDtos.PaginatedResult<BlogDto>> GetAllBlogsAsync(int pageNumber, int pageSize);
        Task<BlogDto> GetBlogByIdAsync(int id);
        



    }
}
