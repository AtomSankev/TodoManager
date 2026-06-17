namespace TodoManager.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public List<TodoItem> TodoItems { get; set; } = new();
}
