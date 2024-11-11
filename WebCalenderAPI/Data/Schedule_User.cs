using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCalenderAPI.Data
{
    [Table("Schedule_User")]
    public class Schedule_User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("schedule_id")]
        //public int schedule_id { get; set; }
        public int ScheduleId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
    }
}
