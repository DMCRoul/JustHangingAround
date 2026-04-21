using JustHangingAround.Models;
using Microsoft.EntityFrameworkCore;

namespace JustHangingAround.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ChatMessageEntity> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}