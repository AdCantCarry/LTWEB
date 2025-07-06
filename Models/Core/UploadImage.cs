using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TechNova.Models.Core
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(upload.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "uploads/news", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/news/{fileName}";

                return Json(new { url = imageUrl });
            }

            return Json(new { error = new { message = "Upload failed" } });
        }
    }
}