using Microsoft.AspNetCore.Mvc;
using TechNova.Models.Data;

namespace TechNova.Controllers
{
    public class AIController : Controller
    {
        private readonly GeminiService _gemini;

        public AIController(GeminiService gemini)
        {
            _gemini = gemini;
        }

        [HttpGet]
        public IActionResult Chat() => View();

        [HttpPost]
        public async Task<IActionResult> Chat(string userMessage)
        {
            var response = await _gemini.AskGeminiAsync(userMessage);
            ViewBag.Response = response;
            ViewBag.UserMessage = userMessage;
            return View();
        }
        private static readonly Dictionary<string, DateTime> UserLastRequest = new();

        [HttpPost]
        public async Task<IActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Vui lòng nhập nội dung câu hỏi." });

            var sessionId = HttpContext.Session.Id;
            if (UserLastRequest.TryGetValue(sessionId, out var lastTime))
            {
                if ((DateTime.Now - lastTime).TotalSeconds < 10)
                    return Json(new { reply = "⏳ Vui lòng chờ 10 giây trước khi hỏi tiếp." });
            }

            UserLastRequest[sessionId] = DateTime.Now;

            try
            {
                var reply = await _gemini.AskGeminiAsync(message);
                return Json(new { reply });
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                return Json(new { reply = "❌ Hệ thống gặp lỗi. Vui lòng thử lại sau." });
            }
        }

    }

}
