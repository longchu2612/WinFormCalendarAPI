﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebCalenderAPI.Data;
using WebCalenderAPI.Models;

namespace WebCalenderAPI.Helper
{
    public class TokenHelper
    {

        private readonly MyDbContext _context;
        private readonly AppSettings _appSettings;

        public TokenHelper(MyDbContext context,IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _context = context;
            _appSettings = optionsMonitor.CurrentValue;
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateFresherToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
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


        
        public async Task<CheckTokenResult> RenewToken(TokenModel model)
        {

            if(String.IsNullOrEmpty(model.RefreshToken) || String.IsNullOrEmpty(model.AccessToken))
            {
                return new CheckTokenResult
                {
                    Status = "401",
                    Error = "AccessToken or RefreshToken invalid"
                };
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeybytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenValidateParam = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeybytes),

                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false 
            };

            try
            {
                var storedToken = _context.RefresherTokens.Include(x => x.user).FirstOrDefault(x => x.Token == model.RefreshToken);
                Console.WriteLine(storedToken);
               
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validateToken);

                if (storedToken == null)
                {

                    return new CheckTokenResult
                    {
                        Status = "401",
                        Error = "RefreshToken is not in database"
                    };
                }
                if (storedToken.IsRevoked == true)
                {
                    return new CheckTokenResult
                    {
                        Status = "401",
                        Error = "RefreshToken deleted"
                    };
                }
                if (storedToken.ExpiredAt < DateTime.Now)
                {
                    var testDate = DateTime.Now;
                    Console.WriteLine(testDate);


                    storedToken.IsRevoked = true;
                    _context.Update(storedToken);
                    await _context.SaveChangesAsync();

                    return new CheckTokenResult
                    {
                        Status = "401",
                        Error = "RefreshToken has end date"
                    };
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
                        return new CheckTokenResult
                        {
                            Status = "401",
                            Error = "Access Token is invalid"

                        };
                    }

                    if (storedToken.JwtId != jti)
                    {
                        return new CheckTokenResult
                        {
                            Status = "401",
                            Error = "Access Token ID does not match Refresh Token ID"
                        };
                    }

                    if (expiredDate.ToLocalTime() > DateTime.Now)
                    {
                        return new CheckTokenResult
                        {
                            Status = "409",
                            Error = "Access Token has not expired"
                        };
                    }
                    else
                    {
                        var access_token = GenerateAccessToken(user);
                        Console.WriteLine(access_token);
                        var tokenInVerificationUpdate = jwtTokenHandler.ValidateToken(access_token, tokenValidateParam, out var validateTokenUpdate);
                        var jtiUpdate = tokenInVerificationUpdate.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                        storedToken.JwtId = jtiUpdate;
                        _context.SaveChanges();

                        return new CheckTokenResult
                        {
                            Status = "200",
                            Error = "Add new access Token",
                            AccessToken = access_token
                        };
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

                return new CheckTokenResult
                {
                    Status = "400",
                    Error = "Something went wrong"
                };
            }


        }
    }
}