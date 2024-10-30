using Microsoft.EntityFrameworkCore;
using WebCalenderAPI.Models;

namespace WebCalenderAPI.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<User> Uses { get; set; }
        public DbSet<Schedule_User> schedule_Users {  get; set; }
        
        public DbSet<RefresherToken> RefresherTokens { get; set; } 

        public DbSet<UserToken> userTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Schedule>(e =>
            {
                e.ToTable("Schedules");
                e.HasKey(sche => sche.Id);
                e.Property(sche => sche.date);
                e.Property(sche => sche.FromX);
                e.Property(sche => sche.FromY);
                e.Property(sche => sche.ToX);
                e.Property(sche => sche.ToY);
                e.Property(sche => sche.Reason);

            });

            modelBuilder.Entity<User>(u =>
            {
                u.ToTable("User");
                u.HasKey(u => u.Id);
                u.Property(u => u.Id).ValueGeneratedOnAdd();
                u.Property(u => u.UserName);
                u.HasIndex(u => u.UserName).IsUnique();
                u.Property(u => u.Password).IsRequired().HasMaxLength(500);
                u.Property(u => u.FullName).IsRequired().HasMaxLength(500);
                u.Property(u => u.Email).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Schedule_User>(
                su =>
                {
                    su.ToTable("ScheduleUser");
                    su.HasKey(su => new { su.user_id,su.schedule_id});
                }

            );
        }
    }


}
