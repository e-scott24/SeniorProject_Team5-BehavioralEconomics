using Microsoft.EntityFrameworkCore;
using DealtHands.Models;

namespace DealtHands.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerChoice> PlayerChoices { get; set; }
        public DbSet<GameChangerEvent> GameChangerEvents { get; set; }
        public DbSet<PlayerGameChanger> PlayerGameChangers { get; set; }
    }
}
