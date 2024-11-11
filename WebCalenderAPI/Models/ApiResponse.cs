namespace WebCalenderAPI.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }   

        public string Message { get; set; }

        //public object Data { get; set; }  

        public string accessToken { get; set; }

        public string refreshToken { get; set; }

        public string errorCode { get; set; }
        
        //public object Refresh_Token { get; set; }

    }
}
