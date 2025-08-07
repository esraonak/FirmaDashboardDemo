using System.Security.Cryptography;
using System.Text;

namespace FirmaDasboardDemo.Helpers
{
    public static class HashHelper
    {
        public static string Hash(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
