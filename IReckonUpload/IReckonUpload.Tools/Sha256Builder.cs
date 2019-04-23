using System;
using System.Security.Cryptography;
using System.Text;

namespace IReckonUpload.Tools
{
    public static class Sha256Builder
    {
        public static string Compute(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString().ToUpper();
        }
    }
}
