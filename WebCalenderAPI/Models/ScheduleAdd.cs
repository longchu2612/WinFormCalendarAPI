using System.ComponentModel.DataAnnotations;

namespace WebCalenderAPI.Models
{
    public class ScheduleAdd
    {

        //[Required]
        public DateTime date { get; set; }
        //[Range(0, 23, ErrorMessage = "Giờ bắt đầu phải từ 0 đến 23.")]
        public int? FromX { get; set; }
        //[Range(0, 59, ErrorMessage = "Phút bắt đầu phải từ 0 đến 59.")]
        public int? FromY { get; set; }
        //[Range(0, 23, ErrorMessage = "Giờ bắt đầu phải từ 0 đến 23.")]
        public int? ToX { get; set; }
        //[Range(0, 59, ErrorMessage = "Phút bắt đầu phải từ 0 đến 59.")]
        public int? ToY { get; set; }
        //[Required(ErrorMessage = "Note không được để trống.")]
        //[StringLength(100, ErrorMessage = "Note không được quá 100 ký tự.")]
        public string Reason { get; set; }
    }
}
