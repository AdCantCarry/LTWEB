using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TechNova.Helpers
{
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData.Add(key, value);
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var query = new StringBuilder();
            foreach (var kv in _requestData)
            {
                query.Append($"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&");
            }

            string signData = query.ToString().TrimEnd('&');
            string secureHash = HmacSHA512(hashSecret, signData);
            return $"{baseUrl}?{signData}&vnp_SecureHash={secureHash}";
        }

        private string HmacSHA512(string key, string input)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new HMACSHA512(keyBytes);
            byte[] hash = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
