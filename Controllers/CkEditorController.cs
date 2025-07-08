using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using System.Threading.Tasks;

namespace TechNova.Controllers
{
    public class CkEditorController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CkEditorController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return Json(new { uploaded = false, error = new { message = "Không có file" } });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ckeditor");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            var url = Url.Content($"~/uploads/ckeditor/{fileName}");
            return Json(new { uploaded = true, url });
        }
    }
}
