using Microsoft.AspNetCore.Http;
using MyMasterpiece.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Infrastructure.Services
{
    public class ImageService: IImageService
    {
        public async Task<string> SaveImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
            var filePathWwwroot = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePathWwwroot, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return $"/images/{uniqueFileName}";
        }
    }
}
