using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
                
                return Ok(_scheduleRepository.GetALl());
            } catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        //private async Task<IActionResult> HandleTokenRefresh()
        //{
        //    string accessToken = _cacheService.GetData<String>("accessToken");

        //    string refreshToken = _cacheService.GetData<String>("refreshToken");

        //    var tokenModel = new TokenModel
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken
        //    };
        //    var checkToken = await _tokenHelper.RenewToken(tokenModel);
        //    if (checkToken.Status == "401")
        //    {
        //        _cacheService.RemoveData("accessToken");
        //        _cacheService.RemoveData("refreshToken");
        //        return Unauthorized(new CheckTokenResult
        //        {
        //            Status = "401",
        //            Error = checkToken.Error
        //        });
        //    }
        //    else if (checkToken.Status == "200")
        //    {
        //        var acccessToken = checkToken.AccessToken;
        //        _cacheService.SetData("accessToken", acccessToken);
        //    }
        //    return null;
        //}



        [HttpGet("getSchedule/{userId}")]
        public async Task<IActionResult> GetScheduleByUserId(int userId)
        {
            try
            {
                //var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                

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
                //var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                //if(authorizationHeader.Count == 0 || authorizationHeader[0].StartsWith("Bearer "))
                //{
                //    return Unauthorized(new CheckTokenResult
                //    {
                //        Error = "AccessToken is missing or not valid"
                //    });
                //}

                //Console.WriteLine(authorizationHeader);

                
               
                
                return Ok(_scheduleRepository.getByDateWithReason(dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        
        }


        [HttpGet("getAllNotify")]
        public async Task<IActionResult> GetAllNotify([FromQuery]DateTime currentTime)
        {
            try
            {
                var userId = _cacheService.GetData<int>("userId");
                var accessToken = _cacheService.GetData<String>("accessToken_" + userId);
                var refreshToken = _cacheService.GetData<RefresherToken>("refreshToken_" + userId);
                if (refreshToken != null && refreshToken.ExpiredAt > DateTime.UtcNow) {
                    if(userId != 0)
                    {
                        return Ok(_scheduleRepository.getAllNotifycation(userId, currentTime));
                    } 
                }
                
                return BadRequest();
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
                CheckTokenResult result = await _tokenHelper.CheckValidateToken(authorizationHeader, userId);
                var response = new SchedulesResponseDTO();
                if (result.Status == "401")
                {
                    return Unauthorized(result.Error);
                }
                else if (result.Status == "201")
                {
                    string accessToken = result.AccessToken;
                    _cacheService.SetData("accessToken_" + userId, accessToken);
                    response.accessToken = accessToken;
                }
                response.scheduleList = _scheduleRepository.getByDateWithReasonWithUserId(userId,dateTime);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        //public async Task<CheckTokenResult> CheckValidateToken(StringValues authorizationHeader, int userId)
        //{
        //    if (authorizationHeader.Count == 0 || !authorizationHeader[0].StartsWith("Bearer "))
        //    {
        //        return new CheckTokenResult
        //        {
        //            Status = "401",
        //            Error = "AccessToken is missing or not valid"
        //        };
        //    }
        //    //get accessToken
        //    var accessToken = authorizationHeader[0].Substring(7);
        //    // get all claims on token
        //    var claimsPrincipal = _tokenHelper.ValidateAccessToken(accessToken);

        //    if (claimsPrincipal == null)
        //    {
        //        return new CheckTokenResult
        //        {
        //            Status = "401",
        //            Error = "AccessToken Invalid"
        //        };
        //    }

        //    var tokenUserId = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
        //    if (string.IsNullOrEmpty(tokenUserId) || tokenUserId != userId.ToString())
        //    {
        //        return new CheckTokenResult
        //        {
        //            Status = "401",
        //            Error = "UserId invalid"
        //        };
        //    }
        //    var utcExpireDate = long.Parse(claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
        //    var expiredDate = convertUnixTimeToDateTime(utcExpireDate);
        //    if (expiredDate.ToLocalTime() < DateTime.Now)
        //    {
        //        var refreshToken = _cacheService.GetData<RefresherToken>("refreshToken_" + tokenUserId);
        //        if (refreshToken.ExpiredAt < DateTime.Now)
        //        {
        //            var user = await _context.Uses.SingleOrDefaultAsync(u => u.Id.ToString() == tokenUserId);
        //            string newAccessToken = _tokenHelper.GenerateAccessToken(user);
        //            return new CheckTokenResult
        //            {
        //                Status = "201",
        //                Error = "Create new acccessToken success",
        //                AccessToken = newAccessToken

        //            };
        //            //_cacheService.SetData("accessToken_" + tokenUserId, newAccessToken);
        //        }
        //        else
        //        {

        //            return new CheckTokenResult
        //            {
        //                Status = "401",
        //                Error = "Token has expried"
        //            };

        //        }

        //    }

        //    return new CheckTokenResult
        //    {
        //        Status = "200",
        //        Error = "Access Token has not expired"
        //    };

        //}

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
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                CheckTokenResult result = await _tokenHelper.CheckValidateToken(authorizationHeader, userId);
                var response = new SchedulesResponseDTO();
                if (result.Status == "401")
                {
                    return Unauthorized(result.Error);
                }
                else if (result.Status == "201")
                {
                    string accessToken = result.AccessToken;
                    response.accessToken = accessToken;
                    _cacheService.SetData("accessToken_" + userId, accessToken);
                }
                response.scheduleList = _scheduleRepository.getAllScheduleWithoutReason(userId, dateTime);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,ScheduleVM scheduleVM)
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];

                int? userId = _scheduleRepository.getUserIdFromSchedule(scheduleVM.id);
                if(userId == null)
                {
                    return NotFound();
                }

                CheckTokenResult result = await _tokenHelper.CheckValidateToken(authorizationHeader, userId);
                var response = new SchedulesResponseDTO();
                if (result.Status == "401")
                {
                    return Unauthorized(result.Error);
                }
                else if (result.Status == "201")
                {
                    string accessToken = result.AccessToken;
                    _cacheService.SetData("accessToken_" + userId, accessToken);
                    response.accessToken = accessToken;
                }
                _scheduleRepository.Update(scheduleVM);
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            try
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                CheckTokenResult result = await _tokenHelper.CheckValidateToken(authorizationHeader, userId);
                var response = new SchedulesResponseDTO();
                if (result.Status == "401")
                {
                    return Unauthorized(result.Error);
                }
                else if (result.Status == "201")
                {
                    string accessToken = result.AccessToken;
                    _cacheService.SetData("accessToken_" + userId, accessToken);
                    response.accessToken = accessToken; 
                }
                _scheduleRepository.Delete(id,userId);
                return Ok(response);
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
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                CheckTokenResult result = await _tokenHelper.CheckValidateToken(authorizationHeader, meata.user_id);
                var response = new SchedulesResponseDTO();
                if (result.Status == "401")
                {
                    return Unauthorized(result.Error);
                }
                else if (result.Status == "201")
                {
                    string accessToken = result.AccessToken;

                    _cacheService.SetData("accessToken_" + meata.user_id, accessToken);

                    response.accessToken = accessToken;
                }
                List<ScheduleVM> listSchedules = new List<ScheduleVM>();
                listSchedules.Add(_scheduleRepository.AddScheduleWithDate(meata));
                response.scheduleList = listSchedules;
                return Ok(response);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



    }
}
