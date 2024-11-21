using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.Business;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class BusinessService:IBusiness
    {
        private readonly ILogger<BusinessService> _logger;
        private readonly EmailHelper _emailHelper;
        private readonly IImageService _imageService;
        private readonly AuctionDbContext _context;
        public BusinessService(AuctionDbContext context, ILogger<BusinessService> logger, EmailHelper emailHelper, IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _emailHelper = emailHelper;
            _imageService = imageService;
        }
        public async Task<navDto> Business()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalProducts = await _context.Products.CountAsync();
                var totalBids = await _context.Bids.CountAsync();

                return new navDto
                {
                    Totaluser = totalUsers,
                    Totalproducts = totalProducts,
                    TotalBids = totalBids
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching business stats.");
                throw;
            }
        }
    }
}
