using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TechNova.Models.Data
{

    public class GeminiService
    {
        private readonly string apiKey = "AIzaSyCxwAno4flCdWnffcapgMCd0VaLht0ACRQ";
        private readonly HttpClient _http;

        public GeminiService(HttpClient httpClient)
        {
            _http = httpClient;
        }
        public async Task<string> AskGeminiAsync(string userInput)
        {
            var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash-002:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text = @$"
Bạn là trợ lý AI cho một website thương mại điện tử chuyên bán đồ công nghệ: điện thoại, laptop, phụ kiện, thiết bị thông minh...

- Luôn tư vấn ngắn gọn, thân thiện, dễ hiểu cho người dùng phổ thông.
- Khi người dùng hỏi chung chung (như 'gợi ý sản phẩm'), hãy đề xuất các loại thiết bị công nghệ phổ biến.
- Nếu không hiểu rõ yêu cầu, hãy hỏi lại để làm rõ.

Người dùng hỏi: {userInput}"
                    }
                }
            }
        }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var res = await _http.PostAsync(url, content);
                var resultString = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    return $"❌ Gemini API lỗi {(int)res.StatusCode}: {res.ReasonPhrase}\n{resultString}";
                }

                dynamic resultJson = JsonConvert.DeserializeObject(resultString);
                string reply = resultJson?.candidates?[0]?.content?.parts?[0]?.text ?? "❌ Không có phản hồi từ Gemini.";
                return reply;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gọi Gemini: " + ex.Message);
                return "❌ Lỗi hệ thống khi gọi Gemini.";
            }
        }


    }
}
