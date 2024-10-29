namespace WebCalenderAPI.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }   

        public string Message { get; set; }

        public object Data_Token { get; set; }  
        
        public object Refresh_Token { get; set; }

    }
}
