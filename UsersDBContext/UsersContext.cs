using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UsersDB;

namespace UsersDBContext
{
    public partial class UsersContext: DbContext
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

        public virtual DbSet<Room> rooms { get; set; }

        public virtual DbSet<Participant> participants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Messages>(entity =>
            {
                entity.ToTable("messages");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");
                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");
                entity.Property(e => e.Message)
                    .HasMaxLength(500)
                    .IsFixedLength()
                    .HasColumnName("message");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_messages_rooms");

                entity.HasOne(d => d.User).WithMany(p => p.Messages)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_messages_users");
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.ToTable("participants");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");
                entity.Property(e => e.DeleteedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleteed_at");
                entity.Property(e => e.LastRead)
                    .HasColumnType("datetime")
                    .HasColumnName("last_read");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Room).WithMany(p => p.Participants)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_participants_rooms");

                entity.HasOne(d => d.User).WithMany(p => p.Participants)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_participants_users");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("rooms");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");
                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");
                entity.Property(e => e.Subject)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("subject");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.login)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("login");
                entity.Property(e => e.password)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .HasColumnName("password");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
