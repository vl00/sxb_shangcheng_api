using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace iSchool.Infras
{
    public static partial class HashAlgmUtil
    {
        public const string Md5 = "md5";
        public const string Sha1 = "sha1";
        public const string Sha256 = "sha256";
        public const string Sha384 = "sha384";
        public const string Sha512 = "sha512";

        public static string Encrypt(string str, string algName, bool islower = true)
        {
            var by = Encrypt(Encoding.UTF8.GetBytes(str), algName);
            var sb = new StringBuilder();
            foreach (var b in by)
                sb.Append(b.ToString(islower ? "x2" : "X2"));
            return sb.ToString();
        }

        public static byte[] Encrypt(byte[] str, string algName)
        {
            using (var ha = HashAlgorithm.Create(algName))
            {
                return ha.ComputeHash(str);
            }
        }
    }
}