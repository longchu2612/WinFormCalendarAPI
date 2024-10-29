using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebCalenderAPI.Data;
using WebCalenderAPI.Models;
using System.Security.Cryptography;

namespace WebCalenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        private readonly AppSettings _appSettings;
        public UserController(MyDbContext context,IOptionsMonitor<AppSettings> optionsMonitor) { 
              _context = context;
              _appSettings =  optionsMonitor.CurrentValue;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            var user = _context.Uses.SingleOrDefault(p => p.UserName == model.userName && p.Password == model.password);
            if(user == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid username/password"
                });
            }
            else
            {

                // cấp token
                var token = await GenerateToken(user);
                var refreshToken = String.Empty;

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Authentication success",
                    Data_Token = token,
                    Refresh_Token = refreshToken
                });
            }
        }

        private async Task<TokenModel> GenerateToken(User user)
        {
            
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeybytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("Id", user.Id.ToString()),

                    //roles

                    

                }),
                Expires = DateTime.UtcNow.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeybytes), SecurityAlgorithms.HmacSha256Signature)
                
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateFresherToken();

            //Luwu trong database

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddHours(1)

            };

            try
            {
                await _context.refreshTokens.AddAsync(refreshTokenEntity);

                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return new TokenModel{
               AccessToken = accessToken,
               RefreshToken = refreshToken
            };
        }

        private string GenerateFresherToken()
        {
            var random = new byte[32];
            using(var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        //[HttpPost("RenewToken")]
        //public async Task<IActionResult> RenewToken(TokenModel model)
        //{

        //}
    }
}
