using Microsoft.EntityFrameworkCore;
using TodoManager.Models;

namespace TodoManager.Services;

public class TodoService
{
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    // ──────────────────────────── CATEGORIES ────────────────────────────

    public async Task<List<Category>> GetAllCategoriesAsync()
        => await _db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetCategoryByIdAsync(int id)
        => await _db.Categories.FindAsync(id);

    public async Task<Category?> GetCategoryByNameAsync(string name)
        => await _db.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

    public async Task<Category> CreateCategoryAsync(string name)
    {
        var category = new Category { Name = name.Trim() };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    // ──────────────────────────── TODO ITEMS ────────────────────────────

    public async Task<List<TodoItem>> GetAllTodosAsync()
    {
        return await _db.TodoItems
            .Include(t => t.Category)
            .OrderBy(t => t.IsDone)
            .ThenBy(t => t.Deadline == null)
            .ThenBy(t => t.Deadline)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetTodoByIdAsync(int id)
        => await _db.TodoItems.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TodoItem> CreateTodoAsync(
        string title, string description, DateTime? deadline,
        Priority priority, int categoryId)
    {
        var item = new TodoItem
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Deadline = deadline,
            Priority = priority,
            CategoryId = categoryId,
            CreatedAt = DateTime.Now,
            IsDone = false
        };
        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateTodoAsync(
        int id, string title, string description, DateTime? deadline,
        Priority priority, int categoryId)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;

        item.Title = title.Trim();
        item.Description = description.Trim();
        item.Deadline = deadline;
        item.Priority = priority;
        item.CategoryId = categoryId;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDoneAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;

        item.IsDone = !item.IsDone;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    // ──────────────────────────── FILTERS ────────────────────────────

    public async Task<List<TodoItem>> GetPendingAsync()
        => await _db.TodoItems
            .Include(t => t.Category)
            .Where(t => !t.IsDone)
            .OrderBy(t => t.Deadline == null)
            .ThenBy(t => t.Deadline)
            .ToListAsync();

    public async Task<List<TodoItem>> GetHighPriorityAsync()
        => await _db.TodoItems
            .Include(t => t.Category)
            .Where(t => t.Priority == Priority.High)
            .OrderBy(t => t.IsDone)
            .ThenBy(t => t.Deadline)
            .ToListAsync();

    public async Task<List<TodoItem>> GetOverdueAsync()
        => await _db.TodoItems
            .Include(t => t.Category)
            .Where(t => !t.IsDone && t.Deadline.HasValue && t.Deadline.Value < DateTime.Now)
            .OrderBy(t => t.Deadline)
            .ToListAsync();

    public async Task<List<TodoItem>> GetByCategoryAsync(int categoryId)
        => await _db.TodoItems
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderBy(t => t.IsDone)
            .ThenBy(t => t.Deadline)
            .ToListAsync();

    // ──────────────────────────── STATISTICS ────────────────────────────

    public async Task<(int Total, int Done, int Pending, int Overdue, int CompletedThisWeek)>
        GetStatsAsync()
    {
        var all = await _db.TodoItems.ToListAsync();
        var now = DateTime.Now;
        var weekAgo = now.AddDays(-7);

        int total = all.Count;
        int done = all.Count(t => t.IsDone);
        int pending = all.Count(t => !t.IsDone);
        int overdue = all.Count(t => !t.IsDone && t.Deadline.HasValue && t.Deadline.Value < now);
        int completedThisWeek = all.Count(t => t.IsDone && t.CreatedAt >= weekAgo);

        return (total, done, pending, overdue, completedThisWeek);
    }
}
