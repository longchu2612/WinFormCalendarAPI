﻿namespace WebCalenderAPI.Data
{
    public class Schedule
    {
        public int Id { get; set; }

        public DateTime date {  get; set; }

        public int? FromX { get; set; }

        public int? FromY { get; set; }

        public int? ToX { get; set; }

        public int? ToY { get; set; }

        public string? Reason { get; set; }

        public ICollection<Schedule_User> schedule_s { get; set; }

        public Schedule()
        {
            schedule_s = new List<Schedule_User>();
        }
    }
}
