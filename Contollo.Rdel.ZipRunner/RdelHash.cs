using System;
using System.IO;
using System.Security.Cryptography;

namespace Contollo.Rdel.ZipRunner
{
    internal static class RdelHash
    {
        public static string ComputeFileSha256(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
