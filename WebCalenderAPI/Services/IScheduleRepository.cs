using WebCalenderAPI.Models;

namespace WebCalenderAPI.Services
{
    public interface IScheduleRepository
    {
        List<ScheduleVM> GetALl();

        ScheduleVM GetById(int id);

        List<ScheduleVM> GetByDate(DateTime dateTime);

        List<ScheduleVM> getByDateWithReason(DateTime dateTime);

        List<ScheduleVM> getByDateWithReasonWithUserId(int userId, DateTime dateTime);

        List<ScheduleVM> getAllScheduleWithoutReason(int userId, DateTime dateTime);

        ScheduleVM Add(ScheduleAdd model);

        void Update(ScheduleVM schedule);

        void Delete(int id);

        ScheduleVM AddScheduleWithDate(ScheduleMeta meta);

        List<ScheduleVM> getAllScheduleByUser(int userId);
    }
}
