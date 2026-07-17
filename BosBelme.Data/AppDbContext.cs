using BosBelme.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BosBelme.Data
{
    //Основной контекст нашей базы данных, который будет использоваться для взаимодействия с таблицами и бд
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Создание таблиц
        public DbSet<User> Users { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<GameHub> GameHubs { get; set; }

        public DbSet<GameSession> GameSessions { get; set; }

        public DbSet<PlayersCount> PlayersCounts { get; set; }

        // Метод для установки связей между таблицами
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameHub>()
                .HasOne(gh => gh.Game)
                .WithMany(g => g.GameHubs)
                .HasForeignKey(gh => gh.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameSession>()
                .HasKey(gs => new { gs.GameHubId, gs.PlayerId });

            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.GameHub)
                .WithMany(gh => gh.GameSessions)
                .HasForeignKey(gs => gs.GameHubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.Player)
                .WithMany(p => p.GameSessions)
                .HasForeignKey(gs => gs.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
