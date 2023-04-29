using iSchool.Infras.Tokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public static class TokenHelper
    {
        public static string CreateStokenByJwt(string key, params object[] args) => CreateStokenByJwt(key, null, null, args);
        public static string CreateStokenByJwt(string key, string alg, params object[] args) => CreateStokenByJwt(key, alg, null, args);

        public static string CreateStokenByJwt(string key, string alg, double? exp, params object[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            var jheader = JwtUtil.CreateHeader(JwtUtil.CreateSigningCredentials(key, alg ?? SecurityAlgorithms.HmacSha256));
            var jpayload = exp == null ? JwtUtil.CreatePayload() : JwtUtil.CreatePayload().AddClaim("_exp", exp);
            for (var i = 0; i < args.Length; i++)
            {
                jpayload = jpayload.AddClaim($"_i_{i + 1}", args[i]);
            }
            var jwtstr = JwtUtil.CreateJwtStr(jheader, jpayload);            
            var span = jwtstr.AsSpan();
            return new string(span[(span.LastIndexOf('.') + 1)..]);
        }

        public static bool ValidStokenByJwt(string key, string token, params object[] args) => ValidStokenByJwt(key, null, null, token, args);
        public static bool ValidStokenByJwt(string key, string alg, string token, params object[] args) => ValidStokenByJwt(key, alg, null, token, args);

        public static bool ValidStokenByJwt(string key, string alg, double? exp, string token, params object[] args)
        {            
            if (args == null) throw new ArgumentNullException(nameof(args));

            return token == CreateStokenByJwt(key, alg, exp, args);

            /* var sc = JwtUtil.CreateSigningCredentials(key, alg);
            var jheader = JwtUtil.CreateHeader(sc);
            var jpayload = exp == null ? JwtUtil.CreatePayload() : JwtUtil.CreatePayload().AddClaim("_exp", exp);
            for (var i = 0; i < args.Length; i++)
            {
                jpayload = jpayload.AddClaim($"_i_{i + 1}", args[i]);
            }
            var jwtstr = $"{jheader.Base64UrlEncode()}.{jpayload.Base64UrlEncode()}.{token}";
            var tokenValidParams = JwtUtil.CreateValidParams(sc, null);
            try
            {
                var (cp, _) = JwtUtil.ValidJwt(jwtstr, tokenValidParams);
                return cp?.Identity?.IsAuthenticated == true;
            }
            catch
            {
                return false;
            } */
        }
    }
}
