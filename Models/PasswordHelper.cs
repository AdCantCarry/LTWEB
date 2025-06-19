using System;
using System.Security.Cryptography;
using System.Text;

namespace TechNova.Helpers
{
    public static class PasswordHelper
    {
        // Hàm mã hóa mật khẩu
        public static string Hash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // Hàm kiểm tra mật khẩu người dùng nhập có trùng với mật khẩu đã mã hóa không
        public static bool Verify(string inputPassword, string hashedPassword)
        {
            string hashedInput = Hash(inputPassword);
            return hashedInput == hashedPassword;
        }
    }
}
