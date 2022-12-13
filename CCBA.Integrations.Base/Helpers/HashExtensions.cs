using System;
using System.Security.Cryptography;

namespace CCBA.Integrations.Base.Helpers
{
    public static class HashExtensions
    {
        public static string ToMd5Hex(this string s)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(s.ToBytes());
            return BitConverter.ToString(hashBytes).Remove("-").ToLower();
        }

        public static string ToSha1(this string s)
        {
            using var sha1Managed = SHA1.Create();
            var hashBytes = sha1Managed.ComputeHash(s.ToBytes());
            return hashBytes.ToBase64String();
        }

        public static string ToSha256(this string s)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(s.ToBytes());
            return hashBytes.ToBase64String();
        }

        public static string ToSha256Hex(this string s)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(s.ToBytes());
            return BitConverter.ToString(hashBytes).Remove("-").ToLower();
        }
    }
}