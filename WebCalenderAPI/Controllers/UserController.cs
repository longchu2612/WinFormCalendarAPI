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
using Microsoft.EntityFrameworkCore;

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
                

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Authentication success",
                    Data = token,
                   
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
                    //new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("Id", user.Id.ToString()),
                    new Claim("TokenId", Guid.NewGuid().ToString()),

                    //roles


                    

                }),
                Expires = DateTime.UtcNow.AddSeconds(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeybytes), SecurityAlgorithms.HmacSha256Signature)
                
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateFresherToken();

            //Luwu trong database

            var refreshTokenEntity = new RefresherToken
            {
                Id = Guid.NewGuid(),
                JwtId = token.Id,
                user = user,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow.ToLocalTime(),
                ExpiredAt = DateTime.UtcNow.ToLocalTime().AddHours(1)

            };

            try
            {
                await _context.RefresherTokens.AddAsync(refreshTokenEntity);
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

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeybytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenValidateParam = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeybytes),

                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false // khong kiem tra het han
            };

            try
            {
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validateToken);

                    // check alg
                    JwtSecurityToken jwtSecurityToken = validateToken as JwtSecurityToken;
                    var result = jwtSecurityToken.Header.Alg.
                        Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                    {
                        return BadRequest(new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid Token"

                        });
                    }


                //Check access token expire
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = convertUnixTimeToDateTime(utcExpireDate);
                if(expireDate > DateTime.UtcNow)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Token has not expired"
                    });
                }

                //check 4: Check refreshtoken in DB
                var storedToken = _context.RefresherTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if(storedToken == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh Token has not expired"
                    });
                }

                //check 5: check refreshtoken is used/ revoked
                if (storedToken.IsUsed)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "The refresh token has is used"
                    });
                }

                if (storedToken.IsRevoked)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "The refresh token has is revoked"
                    });
                }

                //check 6: AccessToken id == jwtId in Refresh token
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if(jti != storedToken.JwtId)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Acces Id not equal Fresh Id"
                    });
                }
                // Update token is used

                storedToken.IsUsed = true;
                storedToken.IsRevoked = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();

                // Create new token
                var user = await _context.Uses.SingleOrDefaultAsync(nd => nd.Id == storedToken.user.Id);
                var token = await GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                });




            }
            catch (Exception ex) {

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            
            }

           
        }

        private DateTime convertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }
    }
}
