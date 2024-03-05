using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyDemo.Enties;

namespace MyDemo.Services
{
    public class JwtService(IOptions<JwtOptions> options)
    {
        private readonly JwtOptions _jwtOptions = options.Value;
        public string GenerateToken(string username)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));

            var userClaimsIdentity = new ClaimsIdentity(claims);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SignKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                IssuedAt = DateTime.Now,
                Subject = userClaimsIdentity,
                Expires = DateTime.Now.AddMinutes(_jwtOptions.ExpireMinutes),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);
            return serializeToken;
        }
    }
}