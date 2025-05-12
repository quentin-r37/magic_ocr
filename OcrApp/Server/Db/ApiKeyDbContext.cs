using Microsoft.EntityFrameworkCore;

namespace OcrApp.Server.Db
{
  
    public class ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options) : DbContext(options)
    {
        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasIndex(a => a.Key)
                    .IsUnique();

                entity.HasIndex(a => a.UserId);

                entity.Property(a => a.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(a => a.ExpiresAt)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString("o") : null,
                        v => v != null ? DateTime.Parse(v) : (DateTime?)null);
            });
        }
    }
}
