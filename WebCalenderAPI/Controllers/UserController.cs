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
using System;

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
                return BadRequest(new ApiResponse
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
                    accessToken = token.AccessToken,
                    refreshToken = token.RefreshToken
                   
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
                Expires = DateTime.UtcNow.ToLocalTime().AddSeconds(30),
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
                IsUsed = true,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow.ToLocalTime(),
                ExpiredAt = DateTime.UtcNow.ToLocalTime().AddMinutes(30)

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

        private string GenerateAccessToken(User user)
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
                Expires = DateTime.UtcNow.ToLocalTime().AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeybytes), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            return accessToken;
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken([FromBody]TokenModel model)
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
                //check 4: Check refreshtoken in DB


                var storedToken = _context.RefresherTokens.Include(x => x.user).FirstOrDefault(x => x.Token == model.RefreshToken);
                Console.WriteLine(storedToken);
                //var user = await _context.Uses.SingleOrDefaultAsync(nd => nd.Id == storedToken.user.Id);


                //if (storedToken == null)
                //{
                //    return NotFound(new ApiResponse { Success = false, Message = "Refresh Token not found in database" });
                //}

                //check 5: check refreshtoken is used/ revoked
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validateToken);
                //if (storedToken.IsUsed == true && storedToken.IsRevoked == false)
                //{
                //    var accessToken = GenerateAccessToken(user);
                //    var jwtId = GetJtiFromToken(accessToken);
                //    storedToken.JwtId = jwtId;
                //    _context.Update(storedToken);
                //    await _context.SaveChangesAsync();
                //    var newToken = new TokenModel
                //    {
                //        AccessToken = accessToken
                //    };
                //    return Ok(new ApiResponse { Success = true, Message = "New access token generated", accessToken = accessToken });
                //}

                //if (storedToken.IsRevoked == true)
                //{

                //    return Unauthorized(new ApiResponse { Success = false, Message = "Refresh token has been revoked" });
                //}

                //check 6: AccessToken id == jwtId in Refresh token
                //---------------

                //var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                //if (jti != storedToken.JwtId)
                //{
                //    return Unauthorized(new ApiResponse { Success = false, Message = "Access Token ID does not match Refresh Token ID" });
                //}
                //-------------------


                //check: Check ExpireAt of refresh token.

                if (storedToken == null)
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "RefreshToken is not in database",
                        errorCode = "TokenNotFound"
                    });

                }
                if(storedToken.IsRevoked == true)
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "RefreshToken deleted",
                        errorCode = "TokenRevoked"
                    });
                }
                if (storedToken.ExpiredAt < DateTime.Now)
                {
                    var testDate = DateTime.Now;
                    Console.WriteLine(testDate);


                    storedToken.IsRevoked = true;
                    _context.Update(storedToken);
                    await _context.SaveChangesAsync();
                    return Unauthorized(new ApiResponse { Success = false, Message = "RefreshToken has end date", errorCode = "TokenExpire" });
                }
                else
                {
                    
                    var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                    var expiredDate = convertUnixTimeToDateTime(utcExpireDate);
                    var user = await _context.Uses.SingleOrDefaultAsync(nd => nd.Id == storedToken.user.Id);
                    JwtSecurityToken jwtSecurityToken = validateToken as JwtSecurityToken;
                    var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return Unauthorized(new ApiResponse {Success = false, Message ="Access Token is invalid", errorCode="AccessTokenInvalid"});
                    }

                    if(storedToken.JwtId != jti)
                    {
                        return Unauthorized(new ApiResponse { Success = false, Message = "Access Token ID does not match Refresh Token ID", errorCode="AccessTokenNotMatch" });
                    }

                    if (expiredDate.ToLocalTime() > DateTime.Now)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, new ApiResponse { Success = false, Message = "Access Token has not expired" });
                    }
                    else
                    {
                        var access_token = GenerateAccessToken(user);
                        Console.WriteLine(access_token);
                        var tokenInVerificationUpdate = jwtTokenHandler.ValidateToken(access_token, tokenValidateParam, out var validateTokenUpdate);
                        var jtiUpdate = tokenInVerificationUpdate.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                        storedToken.JwtId = jtiUpdate;
                        _context.SaveChanges();
                        return Ok(new ApiResponse
                        {
                            Success = true,
                            Message = "Add new access Token",
                            accessToken = access_token
                        });
                    }
                }



                // check alg
                //JwtSecurityToken 
                //jwtSecurityToken = validateToken as JwtSecurityToken;
                //var result = jwtSecurityToken.Header.Alg.
                //    Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                //if (!result)
                //{
                //    return Unauthorized(new ApiResponse
                //    {
                //        Success = false,
                //        Message = "Invalid Token"

                //    });
                //}

                //check 2:Check access token expire
                //var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                //var expiredDate = convertUnixTimeToDateTime(utcExpireDate);
                //if (expiredDate.ToLocalTime() > DateTime.UtcNow.ToLocalTime())
                //{
                //    return StatusCode(StatusCodes.Status409Conflict, new ApiResponse { Success = false, Message = "Access Token has not expired" });
                //}


                

               
                
              
                //if (storedToken.ExpiredAt < DateTime.UtcNow.ToLocalTime())
                //{

                //    var newToken = await GenerateToken(user);

                //    return Ok(new ApiResponse
                //    {
                //        Success = false,
                //        Message = "Refresh has expired",
                //        Data = newToken
                //    });

                //}

                // Update token is used
                
                //-----------------------
                //storedToken.IsUsed = true;
                //storedToken.IsRevoked = true;
                //_context.Update(storedToken);
                //await _context.SaveChangesAsync();

                //--------------------------

                // Create new token
                //--------------------
                //var token = await GenerateToken(user);
                //------------------------------
                //var refreshToken = storedToken.Token;
                //var token = new TokenModel
                //{
                //    AccessToken = accessToken,
                //    RefreshToken = refreshToken,
                //};
                //return Ok(new ApiResponse
                //{
                //    Success = true,
                //    Message = "Renew token success",
                //    accessToken = token.AccessToken,
                //    refreshToken = token.RefreshToken
                //});

            }
            catch (Exception ex) {

                Console.WriteLine(ex.ToString());

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            
            }

           
        }
       

        private string GetJtiFromToken(string accessToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenInVerification = jwtTokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            var jti = tokenInVerification?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            return jti;
        }



        private DateTime convertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval = dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }
    }
}
