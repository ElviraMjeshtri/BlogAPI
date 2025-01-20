using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.FriendlyUrl).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.FriendlyUrl).IsUnique();
        });
    }
}
