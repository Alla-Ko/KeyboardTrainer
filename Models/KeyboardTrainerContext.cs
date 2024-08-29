using Microsoft.EntityFrameworkCore;

namespace KeybordTrainer.Models
{
    public partial class KeyboardTrainerContext : DbContext
    {
        public KeyboardTrainerContext()
        {
        }

        public KeyboardTrainerContext(DbContextOptions<KeyboardTrainerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserLevel> UserLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Task1).HasColumnName("Task1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("userId");
                entity.Property(e => e.UserName).HasColumnName("userName");
            });

            modelBuilder.Entity<UserLevel>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Fails).HasColumnName("fails");
                entity.Property(e => e.Speed).HasColumnName("speed");
                entity.Property(e => e.TaskId).HasColumnName("taskId");
                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLevels)
                    .HasForeignKey(d => d.UserId);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=KeyboardTrainer.db");
            }
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
