using System.ComponentModel.DataAnnotations;

namespace WebCalenderAPI.Models
{
    public class LoginModel
    {
        [Required]
        [MaxLength(50)]
        public string userName {  get; set; }
        [Required] 
        [MaxLength(250)]
        public string password { get; set; }
    }
}
