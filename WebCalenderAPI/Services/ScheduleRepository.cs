using Microsoft.EntityFrameworkCore;
using WebCalenderAPI.Data;
using WebCalenderAPI.Models;

namespace WebCalenderAPI.Services
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly MyDbContext _context;
        public ScheduleRepository(MyDbContext context) {
            _context = context;
        }
        public ScheduleVM Add(ScheduleAdd model)
        {
            var schedule = new Schedule
            {
                date = model.date,
                Reason = model.Reason,
                FromX = model.FromX,
                FromY = model.FromY,
                ToX = model.ToX,
                ToY = model.ToY
            };
            _context.Schedules.Add(schedule);
            _context.SaveChanges();
            return new ScheduleVM
            {
                id = schedule.Id,
                date = schedule.date,
                Reason = schedule.Reason,
                FromX = schedule.FromX,
                FromY = schedule.FromY,
                ToX = schedule.ToX,
                ToY= schedule.ToY
            };
        }

        public ScheduleVM AddScheduleWithDate(DateTime dateTime)
        {
            var schedule = new Schedule { date = dateTime };
            _context.Schedules.Add(schedule);
            _context.SaveChanges();
            return new ScheduleVM
            {
                id = schedule.Id,
                date = schedule.date
            };

        }

        public void Delete(int id)
        {
            var schedule = _context.Schedules.SingleOrDefault(sche => sche.Id == id);
            Console.WriteLine(schedule);
            _context.Schedules.Remove(schedule);
            _context.SaveChanges();
           
        }

        public List<ScheduleVM> GetALl()
        {
            var schedules = _context.Schedules.Select(sche => new ScheduleVM
            {
                id = sche.Id,
                date = sche.date,
                Reason = sche.Reason == null ? "" : sche.Reason,
                FromX = sche.FromX == null ? 0 : sche.FromX,
                FromY = sche.FromY == null ? 0 : sche.FromY,
                ToX = sche.ToX == null ? 0 : sche.ToX,
                ToY = sche.ToY == null ? 0 : sche.ToY
            }).ToList();
            return schedules;
            
        }

        public List<ScheduleVM> GetByDate(DateTime dateTime)
        {
            var schedules = _context.Schedules.Where(sche => sche.date == dateTime).ToList();
            List<ScheduleVM> scheduleVMs = new List<ScheduleVM>();
            foreach (var schedule in schedules) {
                scheduleVMs.Add(new ScheduleVM
                {   id = schedule.Id,
                    date = schedule.date,
                    Reason = schedule.Reason == null ? "" : schedule.Reason,
                    FromX = schedule.FromX == null ? 0 : schedule.FromX,
                    FromY= schedule.FromY == null ? 0 : schedule.FromY,
                    ToX = schedule.ToX == null ? 0 : schedule.ToX,
                    ToY = schedule.ToY == null ? 0 : schedule.ToY
                });
            }
            return scheduleVMs;
        }

        public List<ScheduleVM> getByDateWithReason(DateTime dateTime)
        {
            var schedules = _context.Schedules.Where(sche => sche.date == dateTime && sche.Reason != null).ToList();
            List<ScheduleVM> scheduleVMs = new List<ScheduleVM>();
            foreach (var schedule in schedules)
            {
                scheduleVMs.Add(new ScheduleVM
                {
                    id = schedule.Id,
                    date = schedule.date,
                    Reason = schedule.Reason == null ? "" : schedule.Reason,
                    FromX = schedule.FromX == null ? 0 : schedule.FromX,
                    FromY = schedule.FromY == null ? 0 : schedule.FromY,
                    ToX = schedule.ToX == null ? 0 : schedule.ToX,
                    ToY = schedule.ToY == null ? 0 : schedule.ToY
                });
            }
            return scheduleVMs;

        }

        public ScheduleVM GetById(int id)
        {
            var schedule = _context.Schedules.SingleOrDefault(sche => sche.Id == id);
            if(schedule != null)
            {
                return new ScheduleVM
                {
                   id = schedule.Id,
                   date = schedule.date,
                   Reason = schedule.Reason,
                   FromX = schedule.FromX,
                   FromY = schedule.FromY,
                   ToX = schedule.ToX,
                   ToY = schedule.ToY
                };
            }
            return null;
        }

        public void Update(ScheduleVM schedule)
        {
            var _schedule = _context.Schedules.SingleOrDefault(sche => sche.Id == schedule.id);
                _schedule.Reason = schedule.Reason;
                _schedule.FromX = schedule.FromX;
                _schedule.FromY = schedule.FromY;
                _schedule.ToX = schedule.ToX;
                _schedule.ToY = schedule.ToY;
                _context.SaveChanges();
            
            
        }

       

        
    }
}
