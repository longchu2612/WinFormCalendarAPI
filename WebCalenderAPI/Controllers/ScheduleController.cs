using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCalenderAPI.Data;
using WebCalenderAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebCalenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ScheduleController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult getAll()
        {
            var dsLoai = _context.Schedules.ToList();
            return Ok(dsLoai);
        }

        [HttpGet("{id}")]
        public IActionResult getById(int id)
        {
            var schedule = _context.Schedules.SingleOrDefault(se => se.Id == id);
            if (schedule != null)
            {
                return Ok(schedule);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet("dateTime/{dateTime}")]
        public IActionResult getByDate(DateTime dateTime)
        {
            var schedules = _context.Schedules.Where(se => se.date == dateTime).Select(
                     me => new
                     {
                         Date = me.date,
                         fromX = me.FromX == null ? 0 : me.FromX,
                         fromY = me.FromY == null ? 0 : me.FromY,
                         toX = me.ToX == null ? 0 : me.ToX,
                         toY = me.ToY == null ? 0: me.ToY,
                         reason = me.Reason == null ? "" : me.Reason
                     }

                )
                .ToList();
            if (schedules != null)
            {
                return Ok(schedules);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult createNew(ScheduleModel model)
        {
            try
            {
                var schedule = new Data.Schedule
                {
                    Reason = model.reason,
                    FromX = model.from.X,
                    FromY = model.from.Y,
                    ToX = model.to.X,
                    ToY = model.to.Y,
                    date = model.date.Date
                };
                _context.Add(schedule);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status201Created, schedule);
            }
            catch
            {
                return BadRequest();
            }


        }
        [HttpPut("{id}")]
        public IActionResult UpdateScheduleById(int id, ScheduleModel model)
        {
            var schedule = _context.Schedules.SingleOrDefault(lo => lo.Id == id);

            if (schedule != null)
            {
                schedule.Reason = model.reason;
                schedule.FromX = model.from.X;
                schedule.ToX = model.to.X;
                schedule.FromY = model.from.Y;
                schedule.ToY = model.to.Y;  
                _context.SaveChanges();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult deleteById(int id)
        {
            var schedule = _context.Schedules.SingleOrDefault(lo => lo.Id == id);
            if (schedule != null)
            {
                _context.Remove(schedule);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                return NotFound();
            }

        }


    }
}
