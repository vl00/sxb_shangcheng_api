using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace iSchool.Infras.Tokens
{
    /// <summary>
    /// <code>
    /// <br/> JwtUtil.CreateJwtStr(
    /// <br/>     JwtUtil.CreateHeader(JwtUtil.CreateSigningCredentials("key", "alg"))
    /// <br/>         .AddHeader("prop", "value"),
    /// <br/>     JwtUtil.CreatePayload(300)
    /// <br/>         .AddClaim("prop", "value")
    /// <br/> );
    /// <br/> 
    /// <br/> JwtPayload.Deserialize("payload_json");
    /// <br/> 
    /// <br/> JwtUtil.GetJwtToken("jwt");
    /// <br/> 
    /// <br/> var tokenValidParams = JwtUtil.CreateValidParams(JwtUtil.CreateSigningCredentials("key", "alg"), exp)
    /// <br/> tokenValidParams.XXX = XXX; //.
    /// <br/> 
    /// <br/> JwtUtil.ValidJwt("jwt", tokenValidParams);
    /// </code>
    /// </summary>
    public static class JwtUtil
    {
        
    }
}
