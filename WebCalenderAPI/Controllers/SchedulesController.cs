using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCalenderAPI.Models;
using WebCalenderAPI.Services;

namespace WebCalenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;

        public SchedulesController(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            try {
                return Ok(_scheduleRepository.GetALl());
            } catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
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
        public IActionResult GetByDateTime(DateTime dateTime)
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
        public IActionResult GetByDateTimeWithHavingReason(DateTime dateTime) {
            try {

                return Ok(_scheduleRepository.getByDateWithReason(dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        
        } 


        [HttpPut("{id}")]
        public IActionResult Update(int id,ScheduleVM scheduleVM)
        {
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
        public IActionResult Delete(int id)
        {
            try
            {
                _scheduleRepository.Delete(id);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult Add(ScheduleAdd scheduleAdd)
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

        [HttpPost("{dateTime}")]
        public IActionResult AddScheduleWithDate(DateTime dateTime)
        {
            try
            {
                return Ok(_scheduleRepository.AddScheduleWithDate(dateTime));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
