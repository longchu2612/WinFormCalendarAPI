namespace WebCalenderAPI.Models
{
    public class UserToken
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 

        public string accessToken { get; set; }

        public string ExpiredDateAccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string ExpiredRefreshToken { get; set; }

        public DateTime createdDate { get; set; }

        public Boolean isActive { get; set; }
    }
}
