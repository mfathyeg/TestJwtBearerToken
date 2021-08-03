using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TestJwtBearerToken.Data;

namespace TestJwtBearerToken.Utilities
{
    public class TokenManager
    {
        public  static string GenerateRefreshToken()
        {
            var randomNumber = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public static string GenerateAccessToken(MyUser user, string key, int lifeTimeMinutes, string audience, string issuer )
        {
            var utcNow = DateTime.UtcNow;
            var claimsValues = new Dictionary<string, string>();
            claimsValues[JwtRegisteredClaimNames.Sub] = user.Id;
            claimsValues[JwtRegisteredClaimNames.UniqueName] = user.UserName;
            claimsValues[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString();
            claimsValues[JwtRegisteredClaimNames.Iat] = utcNow.ToString();
              
            var claimsList = new List<Claim>();
            foreach (KeyValuePair<string, string> i in claimsValues)
                claimsList.Add(new Claim(i.Key, i.Value));
            var claims = claimsList.ToArray();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddMinutes(lifeTimeMinutes),
                audience: audience,
                issuer: issuer
                );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public static ClaimsPrincipal GetExpiredToken(string token, string key)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
