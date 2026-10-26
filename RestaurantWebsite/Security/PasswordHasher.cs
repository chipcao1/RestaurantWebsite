using System;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantWebsite.Security
{
    /// <summary>
    /// Password Hasher cung cấp thuật toán băm PBKDF2 (HMAC-SHA256) an toàn với Salt riêng,
    /// đồng thời hỗ trợ tương thích ngược với định dạng SHA-256 legacy của hệ thống cũ.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltByteSize = 16;         // 128-bit salt
        private const int HashByteSize = 32;         // 256-bit subkey
        private const int Pbkdf2Iterations = 50000;  // 50,000 iterations (chuẩn an toàn cân bằng hiệu năng ASP.NET)
        private const string VersionPrefix = "$PBKDF2$V1$";

        /// <summary>
        /// Tạo chuỗi hash mới cho mật khẩu theo chuẩn PBKDF2 với Salt ngẫu nhiên riêng biệt.
        /// Định dạng: $PBKDF2$V1${iterations}${salt_base64}${hash_base64}
        /// Độ dài kết quả: ~80-85 ký tự (phù hợp với nvarchar(max) / nvarchar).
        /// </summary>
        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[SaltByteSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] subkey;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations))
            {
                subkey = pbkdf2.GetBytes(HashByteSize);
            }

            string saltBase64 = Convert.ToBase64String(salt);
            string subkeyBase64 = Convert.ToBase64String(subkey);

            return $"{VersionPrefix}{Pbkdf2Iterations}${saltBase64}${subkeyBase64}";
        }

        /// <summary>
        /// Xác minh mật khẩu với chuỗi hash đã lưu trong database.
        /// Hỗ trợ cả định dạng mới ($PBKDF2$V1$...) và định dạng SHA-256 legacy (64 ký tự hex).
        /// </summary>
        /// <param name="hashedPassword">Chuỗi hash lấy từ database</param>
        /// <param name="providedPassword">Mật khẩu người dùng nhập</param>
        /// <param name="needsRehash">Trả về true nếu hash cũ là legacy SHA-256 cần được nâng cấp tự động</param>
        public static bool VerifyPassword(string hashedPassword, string providedPassword, out bool needsRehash)
        {
            needsRehash = false;

            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }

            // 1. Kiểm tra định dạng PBKDF2 V1
            if (hashedPassword.StartsWith(VersionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string payload = hashedPassword.Substring(VersionPrefix.Length);
                    string[] parts = payload.Split('$');
                    if (parts.Length != 3)
                    {
                        return false;
                    }

                    int iterations = int.Parse(parts[0]);
                    byte[] salt = Convert.FromBase64String(parts[1]);
                    byte[] expectedSubkey = Convert.FromBase64String(parts[2]);

                    byte[] actualSubkey;
                    using (var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, iterations))
                    {
                        actualSubkey = pbkdf2.GetBytes(expectedSubkey.Length);
                    }

                    return ByteArraysEqual(expectedSubkey, actualSubkey);
                }
                catch
                {
                    return false;
                }
            }

            // 2. Kiểm tra định dạng Legacy SHA-256 (64 hex characters)
            if (hashedPassword.Length == 64)
            {
                string legacyHash = ComputeLegacySha256(providedPassword);
                if (string.Equals(hashedPassword, legacyHash, StringComparison.OrdinalIgnoreCase))
                {
                    needsRehash = true; // Xác thực đúng, đánh dấu cần nâng cấp lên PBKDF2
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra nhanh mật khẩu đúng (không quan tâm rehash).
        /// </summary>
        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return VerifyPassword(hashedPassword, providedPassword, out _);
        }

        /// <summary>
        /// Băm theo cơ chế SHA-256 cũ dùng riêng cho tương thích ngược.
        /// </summary>
        public static string ComputeLegacySha256(string password)
        {
            if (password == null) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// So sánh hai mảng byte theo thời gian không đổi (Constant-time comparison) để phòng chống tấn công Timing Attack.
        /// </summary>
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
