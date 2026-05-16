using BookQuotes.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookQuotes.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .HasMaxLength(50);

        modelBuilder.Entity<Book>()
            .Property(b => b.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<Book>()
            .Property(b => b.Author)
            .HasMaxLength(200);

        modelBuilder.Entity<Quote>()
            .Property(q => q.Text)
            .HasMaxLength(1000);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // USER → QUOTES (1-to-many)
        modelBuilder.Entity<Quote>()
            .HasOne(q => q.User)
            .WithMany(u => u.Quotes)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Quote>()
            .HasIndex(q => q.UserId);

        // UNIQUE constraint for Books (Title + Author)
        modelBuilder.Entity<Book>()
            .HasIndex(b => new { b.Title, b.Author })
            .IsUnique();
    }
}
