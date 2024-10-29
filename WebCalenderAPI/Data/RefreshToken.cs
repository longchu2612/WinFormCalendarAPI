using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebCalenderAPI.Data
{
    public class RefreshToken
    {
       
        public Guid Id { get; set; }
        public User user { get; set; }

        // Refreshe Token
        public string Token { get; set; }
        // Access Token
        public string JwtId { get; set; }
        // đã được sử dụng?
        public bool IsUsed { get; set; }
        // đã được thu hồi
        public bool? IsRevoked { get; set; }
        // đưiọc tạo ngày nào?
        public DateTime? IssuedAt { get; set; }
        //hết hạn vào lúc nào
        public DateTime? ExpiredAt { get; set; }
    }
}
