using Microsoft.EntityFrameworkCore;
using TodoManager.Models;

namespace TodoManager;

public class TodoDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=todo.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        // TodoItem configuration
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(t => t.Description)
                  .HasMaxLength(1000);
            entity.Property(t => t.Priority)
                  .HasConversion<int>();

            // One-to-many relationship: Category -> TodoItems
            entity.HasOne(t => t.Category)
                  .WithMany(c => c.TodoItems)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

