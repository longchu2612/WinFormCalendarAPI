using WebCalenderAPI.Models;

namespace WebCalenderAPI.Services
{
    public interface IScheduleRepository
    {
        List<ScheduleVM> GetALl();

        ScheduleVM GetById(int id);

        List<ScheduleVM> GetByDate(DateTime dateTime);

        List<ScheduleVM> getByDateWithReason(DateTime dateTime);

        ScheduleVM Add(ScheduleAdd model);

        void Update(ScheduleVM schedule);

        void Delete(int id);

        ScheduleVM AddScheduleWithDate(DateTime dateTime);

        
    }
}
