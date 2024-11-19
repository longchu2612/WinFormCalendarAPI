using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebCalenderAPI.Data;
using WebCalenderAPI.Helper;
using WebCalenderAPI.Models;
using WebCalenderAPI.Services;

namespace WebCalenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ICacheService _cacheService;
        private readonly TokenHelper _tokenHelper;
        private readonly AppSettings _appSettings;
        private readonly MyDbContext _context;

        public SchedulesController(IScheduleRepository scheduleRepository, IOptionsMonitor<AppSettings> optionsMonitor, ICacheService cacheService, TokenHelper tokenHelper, MyDbContext context)
        {
            _scheduleRepository = scheduleRepository;
            _cacheService = cacheService;   
            _tokenHelper = tokenHelper;
            _appSettings = optionsMonitor.CurrentValue;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null) {
                    return tokenResult;
                }
                return Ok(_scheduleRepository.GetALl());
            } catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> HandleTokenRefresh()
        {
            string accessToken = _cacheService.GetData<String>("accessToken");

            string refreshToken = _cacheService.GetData<String>("refreshToken");

            var tokenModel = new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            var checkToken = await _tokenHelper.RenewToken(tokenModel);
            if (checkToken.Status == "401")
            {
                _cacheService.RemoveData("accessToken");
                _cacheService.RemoveData("refreshToken");
                return Unauthorized(new CheckTokenResult
                {
                    Status = "401",
                    Error = checkToken.Error
                });
            }
            else if (checkToken.Status == "200")
            {
                var acccessToken = checkToken.AccessToken;
                _cacheService.SetData("accessToken", acccessToken);
            }
            return null;
        }



        [HttpGet("getSchedule/{userId}")]
        public async Task<IActionResult> GetScheduleByUserId(int userId)
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                Console.WriteLine(authorizationHeader[0]);
                Console.WriteLine(authorizationHeader[1]);

                //var tokenResult = await HandleTokenRefresh();
                //if (tokenResult != null)
                //{
                //    return tokenResult;

                //}
                return Ok(_scheduleRepository.getAllScheduleByUser(userId));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.GetById(id));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("dateTime/{dateTime}")]
        public async Task<IActionResult> GetByDateTime(DateTime dateTime)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.GetByDate(dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [HttpGet("getAllDate/{dateTime}")]
        public async Task<IActionResult> GetByDateTimeWithHavingReason(DateTime dateTime) {
            try {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                //if(authorizationHeader.Count == 0 || authorizationHeader[0].StartsWith("Bearer "))
                //{
                //    return Unauthorized(new CheckTokenResult
                //    {
                //        Error = "AccessToken is missing or not valid"
                //    });
                //}

                Console.WriteLine(authorizationHeader);

                
               
                
                return Ok(_scheduleRepository.getByDateWithReason(dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        
        }

        [HttpGet("getAllDateByUserId")]
        public async Task<IActionResult> GetByDateTimeByUserIdWithHavingReason([FromQuery]DateTime dateTime,[FromQuery] int userId)
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                if (authorizationHeader.Count == 0 || authorizationHeader[0].StartsWith("Bearer "))
                {
                    return Unauthorized(new CheckTokenResult
                    {
                        Error = "AccessToken is missing or not valid"
                    });
                }
                //get accessToken
                var accessToken = authorizationHeader[0].Substring(7);
                // get all claims on token
                var claimsPrincipal = ValidateAccessToken(accessToken);
                if(claimsPrincipal == null)
                {
                    return Unauthorized(new CheckTokenResult
                    {
                        Error = "AccessToken Invalid"
                    });
                }

                var tokenUserId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                if(string.IsNullOrEmpty(tokenUserId) || tokenUserId != userId.ToString())
                {
                    return Unauthorized(new CheckTokenResult
                    {
                        Error = "UserId invalid"
                    });
                }
                var utcExpireDate = long.Parse(claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiredDate = convertUnixTimeToDateTime(utcExpireDate);
                if(expiredDate.ToLocalTime() < DateTime.Now)
                {
                    var refreshToken = _cacheService.GetData<RefresherToken>("refreshToken_"+tokenUserId);
                    if(refreshToken.ExpiredAt < DateTime.Now)
                    {
                        var user = await _context.Uses.SingleOrDefaultAsync(u => u.Id.ToString() == tokenUserId);
                        string newAccessToken = GenerateAccessToken(user);
                        _cacheService.SetData("accessToken_" + tokenUserId, newAccessToken);
                    }
                    else
                    {
                        return Unauthorized(new CheckTokenResult
                        {
                            Error = "Token has expried"
                        });
                    }

                }
                return Ok(_scheduleRepository.getByDateWithReasonWithUserId(userId, dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        public string GenerateAccessToken(User user)
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

        private ClaimsPrincipal ValidateAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_appSettings.SecretKey);


                var tokenValidateParam = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false, // kiem tra nguồn phát hành
                    ValidateAudience = false, // kiểm tra ngưới nhận

                    ValidateIssuerSigningKey = true, //Kiểm tra chữ ký
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ClockSkew = TimeSpan.Zero, // loại bỏ thời gian chênh lệch mặc định 5 phút
                    ValidateLifetime = false // khong kiem tra het han
                };

                var pricipal = tokenHandler.ValidateToken(accessToken, tokenValidateParam, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return pricipal;
                }


            }
            catch (Exception ex) {
                Console.WriteLine($"Token validation failed: {ex.Message}");
            }

            return null;
            
        }

        private DateTime convertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval = dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }

        [HttpGet("getAllDateByUserIdWithoutReason")]
        public async Task<IActionResult> GetByDateTimeByUserIdWithoutReason([FromQuery] DateTime dateTime, [FromQuery] int userId)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.getAllScheduleWithoutReason(userId, dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,ScheduleVM scheduleVM)
        {
            var tokenResult = await HandleTokenRefresh();
            if (tokenResult != null)
            {
                return tokenResult;

            }
            if (id != scheduleVM.id)
            {
                return BadRequest();
            }
            try
            {
                _scheduleRepository.Update(scheduleVM);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                _scheduleRepository.Delete(id);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        
        public async Task<IActionResult> Add(ScheduleAdd scheduleAdd)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.Add(scheduleAdd));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("insertByDate")]
        public async Task<IActionResult> AddScheduleWithDate([FromBody] ScheduleMeta meata)
        {
            try
            {
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.AddScheduleWithDate(meata));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



    }
}
