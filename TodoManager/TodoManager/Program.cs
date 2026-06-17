using Microsoft.EntityFrameworkCore;
using TodoManager.Services;

namespace TodoManager;

class Program
{
    static async Task Main(string[] args)
    {
        // Set console encoding to support Ukrainian text and Unicode symbols
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        // Initialize DB and apply migrations
        using var db = new TodoDbContext();

        try
        {
            //await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ПОМИЛКА] Не вдалося ініціалізувати базу даних: {ex.Message}");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        var service = new TodoService(db);
        var menu = new MenuHandler(service);

        try
        {
            await menu.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[КРИТИЧНА ПОМИЛКА] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}
