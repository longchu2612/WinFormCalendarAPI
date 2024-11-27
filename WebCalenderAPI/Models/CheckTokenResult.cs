namespace WebCalenderAPI.Models
{
    public class CheckTokenResult
    {
        public string Status { get; set; }

        public string Error { get; set; }

        public string AccessToken { get; set; }

        public string ErrorCode { get; set; }
    }
}
