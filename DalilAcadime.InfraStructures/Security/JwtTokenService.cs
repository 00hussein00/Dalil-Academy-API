using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
namespace DalilAcadime.InfraStructures.Security
{
    public class JwtTokenService(IOptions<JwtSetting> option) : ITokenService
    {
        public (string token, DateTime expiration) GenerateToken(Users user)
        {
            return GenerateToken(user.Id, user.Email, user.Role);
        }

        public (string token, DateTime expiration) GenerateToken(Student student)
        {
            return GenerateToken(student.Id, student.Email, student.Role);
        }

        private (string token, DateTime expiration) GenerateToken(int id, string email, string role)
        {
            var jwt = option.Value;
            var expiratAt = DateTime.UtcNow.AddHours(jwt.ExpiresInHours);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                Expires = expiratAt,
                // claims identity its a collection of claims that represent the user or entity for which the token is being generated. In this case, we are creating a ClaimsIdentity with a single claim that represents the user's email address.
                Subject = new ClaimsIdentity(
                [
                        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Role, role),

                ]),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    SecurityAlgorithms.HmacSha256
                )
            };
            return (new JsonWebTokenHandler().CreateToken(descriptor), expiratAt);
        }

    }
}
