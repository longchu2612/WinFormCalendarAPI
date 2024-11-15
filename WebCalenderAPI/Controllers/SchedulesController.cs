using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SchedulesController(IScheduleRepository scheduleRepository, ICacheService cacheService, TokenHelper tokenHelper)
        {
            _scheduleRepository = scheduleRepository;
            _cacheService = cacheService;   
            _tokenHelper = tokenHelper;
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
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
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
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
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
                var tokenResult = await HandleTokenRefresh();
                if (tokenResult != null)
                {
                    return tokenResult;

                }
                return Ok(_scheduleRepository.getByDateWithReasonWithUserId(userId, dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
