using System.Drawing;

namespace WebCalenderAPI.Models
{
    public class ScheduleModel
    {
        public string reason { get; set; }

        public DateTime date { get; set; }

        public Point from { get; set; }

        public Point to { get; set; }
    }
}
