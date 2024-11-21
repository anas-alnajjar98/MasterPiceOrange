using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.BlogDtos;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class HomeService:IHome
    {
        private readonly AuctionDbContext _context;
        private readonly ILogger<HomeService> _logger;
        public HomeService(AuctionDbContext context, ILogger<HomeService> logger)
        {
            _context = context;
            _logger = logger;

        }
        public async Task<PaginatedResult<BlogDto>> GetAllBlogsAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching all blogs. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

            try
            {
                var totalBlogs = await _context.Blogs.CountAsync();
                var totalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);

                var blogs = await _context.Blogs
                    .Include(b => b.Author)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(blog => new MyMasterpiece.Application.DTOs.BlogDtos.BlogDto
                    {
                        BlogId = blog.BlogId,
                        Title = blog.Title,
                        Content = blog.Content,
                        PublishedAt = blog.PublishedAt,
                        ViewCount = blog.ViewCount,
                        ImageUrl = blog.ImageUrl,
                        ApprovalStatus = blog.ApprovalStatus,
                        AuthorName = blog.Author.Username,
                        AuthorAvatar = blog.Author.ImageUrl
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully fetched {BlogCount} blogs.", blogs.Count);

                return new MyMasterpiece.Application.DTOs.BlogDtos.PaginatedResult<BlogDto>
                {
                    Data = blogs,
                    TotalPages = totalPages,
                    TotalCount = totalBlogs,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching blogs.");
                throw new InvalidOperationException("Failed to fetch blogs. Please try again later.", ex);
            }
        }
        public async Task<BlogDto> GetBlogByIdAsync(int id)
        {
            _logger.LogInformation("Fetching blog with ID {BlogId}.", id);

            try
            {
                var blog = await _context.Blogs
                    .Include(b => b.Author)
                    .Where(b => b.BlogId == id)
                    .Select(blog => new MyMasterpiece.Application.DTOs.BlogDtos.BlogDto
                    {
                        BlogId = blog.BlogId,
                        Title = blog.Title,
                        Content = blog.Content,
                        PublishedAt = blog.PublishedAt,
                        ViewCount = blog.ViewCount,
                        ApprovalStatus = blog.ApprovalStatus,
                        ImageUrl = blog.ImageUrl,
                        AuthorName = blog.Author.Username,
                        AuthorAvatar = blog.Author.ImageUrl
                    })
                    .FirstOrDefaultAsync();

                if (blog == null)
                {
                    _logger.LogWarning("Blog with ID {BlogId} not found.", id);
                    return null;
                }

                _logger.LogInformation("Blog with ID {BlogId} fetched successfully.", id);
                return blog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching blog with ID {BlogId}.", id);
                throw new InvalidOperationException("Failed to fetch blog. Please try again later.", ex);
            }
        }
    }

}
