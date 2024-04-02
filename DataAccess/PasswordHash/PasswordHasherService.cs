using System.Security.Cryptography;

namespace DataAccess.PasswordHash
{
    public class PasswordHasherService : IPasswordHasherService
    {
        // Numbers forloop to increase the difficulty for finding
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;
        public string HashPassword(string password)
        {
            // Tạo ra một salt ngẫu nhiên
            byte[] salt = new byte[SaltSize];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Tạo một hash từ mật khẩu và salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Kết hợp salt và hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Chuyển đổi byte array thành string và trả về
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Lấy salt từ mật khẩu đã mã hóa
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Tạo một hash từ mật khẩu và salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // So sánh hash được tạo ra từ mật khẩu nhập vào và hash từ mật khẩu đã mã hóa
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }
           
    }
}
