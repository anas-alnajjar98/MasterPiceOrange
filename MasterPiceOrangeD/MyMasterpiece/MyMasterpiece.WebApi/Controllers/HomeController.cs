using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.Interfaces;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHome _Home;
        public HomeController(ILogger<HomeController> logger, IHome Home)
        {
            _logger = logger;
            _Home = Home;

        }
        [HttpGet("GetAllBlogs")]
        public async Task<IActionResult> GetAllBlogs(int pageNumber = 1, int pageSize = 6)
        {
            _logger.LogInformation("API call: GetAllBlogs. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

            try
            {
                var result = await _Home.GetAllBlogsAsync(pageNumber, pageSize);

                if (!result.Data.Any())
                {
                    _logger.LogWarning("No blogs found.");
                    return NotFound(new { message = "No blogs found." });
                }

                return Ok(new
                {
                    Blogs = result.Data,
                    result.TotalPages,
                    result.TotalCount,
                    result.PageNumber,
                    result.PageSize
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "An error occurred while processing GetAllBlogs.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("GetBlogById/{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            _logger.LogInformation("API call: GetBlogById with ID {BlogId}.", id);

            try
            {
                var blog = await _Home.GetBlogByIdAsync(id);

                if (blog == null)
                {
                    return NotFound(new { message = "Blog not found." });
                }

                return Ok(blog);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error occurred in GetBlogById API.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
