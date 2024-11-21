using Microsoft.AspNetCore.Mvc;
using MyMasterpiece.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyMasterpiece.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusiness _businessService;

        public BusinessController(IBusiness businessService)
        {
            _businessService = businessService;
        }

        [HttpGet("GetBusinessStats")]
        public async Task<IActionResult> GetBusinessStats()
        {
            try
            {
                var stats = await _businessService.Business();

                if (stats == null)
                {
                    return NotFound(new { message = "Business stats not found." });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching business stats.", details = ex.Message });
            }
        }
    }
}
