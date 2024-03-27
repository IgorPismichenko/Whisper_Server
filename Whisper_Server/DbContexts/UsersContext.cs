using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Whisper_Server.DbContexts
{
    public class UsersContext: DbContext
    {
        static DbContextOptions<UsersContext> _options;

        static UsersContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<UsersContext>();
            _options = optionsBuilder.UseSqlServer(connectionString).Options;
        }

        public UsersContext()
            : base(_options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Users> users { get; set; }
        public DbSet<Messages> messages { get; set; }
        public DbSet<BlackList> blackList { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlackList>()
                .HasOne(b => b.BlockerUser)
                .WithMany()
                .HasForeignKey(b => b.BlockerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlackList>()
                .HasOne(b => b.BlockedUser)
                .WithMany()
                .HasForeignKey(b => b.BlockedUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Messages>()
                .HasOne(b => b.SenderUser)
                .WithMany()
                .HasForeignKey(b => b.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(b => b.ReceiverUser)
                .WithMany()
                .HasForeignKey(b => b.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
