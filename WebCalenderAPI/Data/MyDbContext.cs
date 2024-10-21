using Microsoft.EntityFrameworkCore;

namespace WebCalenderAPI.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Schedule> Schedules { get; set; }

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
        }
    }


}
