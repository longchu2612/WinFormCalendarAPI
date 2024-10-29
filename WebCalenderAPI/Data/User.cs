namespace WebCalenderAPI.Data
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }    

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }   

        public ICollection<Schedule_User> schedule_Users { get; set; }

        public User()
        {
            schedule_Users = new List<Schedule_User>();
        }
    }
}
