using TodoManager.Models;

namespace TodoManager.Services;

public static class ConsoleHelper
{
    // ──────────────────────────── COLOR HELPERS ────────────────────────────

    public static void WriteColored(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = prev;
    }

    public static void WriteLineColored(string text, ConsoleColor color)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }

    public static void WriteSuccess(string text) => WriteLineColored(text, ConsoleColor.Green);
    public static void WriteError(string text) => WriteLineColored(text, ConsoleColor.Red);
    public static void WriteWarning(string text) => WriteLineColored(text, ConsoleColor.Yellow);
    public static void WriteInfo(string text) => WriteLineColored(text, ConsoleColor.Cyan);
    public static void WriteHeader(string text) => WriteLineColored(text, ConsoleColor.Magenta);

    // ──────────────────────────── DIVIDERS ────────────────────────────

    public static void PrintDivider(char ch = '─', int width = 80)
        => WriteLineColored(new string(ch, width), ConsoleColor.DarkGray);

    public static void PrintTitle(string title)
    {
        Console.WriteLine();
        PrintDivider('═');
        WriteLineColored($"  {title}", ConsoleColor.Magenta);
        PrintDivider('═');
    }

    public static void PrintSectionTitle(string title)
    {
        Console.WriteLine();
        WriteLineColored($"  ── {title} ──", ConsoleColor.Cyan);
        PrintDivider();
    }

    // ──────────────────────────── TODO TABLE ────────────────────────────

    public static void PrintTodoTable(List<TodoItem> items)
    {
        if (items.Count == 0)
        {
            WriteWarning("  (немає завдань для відображення)");
            return;
        }

        // Header
        WriteColored($"  {"ID",-5}", ConsoleColor.DarkGray);
        WriteColored($"{"Статус",-9}", ConsoleColor.DarkGray);
        WriteColored($"{"Назва",-28}", ConsoleColor.DarkGray);
        WriteColored($"{"Пріоритет",-11}", ConsoleColor.DarkGray);
        WriteColored($"{"Категорія",-15}", ConsoleColor.DarkGray);
        WriteColored($"{"Дедлайн",-18}", ConsoleColor.DarkGray);
        Console.WriteLine();
        PrintDivider();

        var now = DateTime.Now;

        foreach (var item in items)
        {
            bool isOverdue = !item.IsDone && item.Deadline.HasValue && item.Deadline.Value < now;
            bool isDone = item.IsDone;

            ConsoleColor rowColor = isDone ? ConsoleColor.Green
                                  : isOverdue ? ConsoleColor.Red
                                  : ConsoleColor.White;

            string status = isDone ? "[✓]" : isOverdue ? "[!]" : "[ ]";
            string deadline = item.Deadline.HasValue
                ? item.Deadline.Value.ToString("dd.MM.yyyy HH:mm")
                : "—";

            string priority = item.Priority switch
            {
                Priority.High => "Високий",
                Priority.Medium => "Середній",
                Priority.Low => "Низький",
                _ => "?"
            };

            string title = item.Title.Length > 26 ? item.Title[..23] + "..." : item.Title;
            string category = (item.Category?.Name ?? "?").Length > 13
                ? (item.Category?.Name ?? "?")[..10] + "..."
                : (item.Category?.Name ?? "?");

            WriteColored($"  {item.Id,-5}", ConsoleColor.DarkGray);
            WriteColored($"{status,-9}", rowColor);
            WriteColored($"{title,-28}", rowColor);

            // Priority color
            ConsoleColor priorityColor = item.Priority switch
            {
                Priority.High => ConsoleColor.Red,
                Priority.Medium => ConsoleColor.Yellow,
                Priority.Low => ConsoleColor.Green,
                _ => ConsoleColor.White
            };
            WriteColored($"{priority,-11}", isDone ? ConsoleColor.DarkGreen : priorityColor);
            WriteColored($"{category,-15}", rowColor);
            WriteColored($"{deadline,-18}", isOverdue && !isDone ? ConsoleColor.Red : rowColor);
            Console.WriteLine();
        }

        PrintDivider();
        Console.WriteLine($"  Всього: {items.Count} | " +
                          $"Виконано: {items.Count(t => t.IsDone)} | " +
                          $"Активних: {items.Count(t => !t.IsDone)}");
    }

    // ──────────────────────────── CATEGORY TABLE ────────────────────────────

    public static void PrintCategoryTable(List<Category> categories)
    {
        if (categories.Count == 0)
        {
            WriteWarning("  (немає категорій)");
            return;
        }

        WriteColored($"  {"ID",-6}", ConsoleColor.DarkGray);
        WriteColored($"{"Назва категорії",-30}", ConsoleColor.DarkGray);
        WriteColored($"{"К-сть завдань",-15}", ConsoleColor.DarkGray);
        Console.WriteLine();
        PrintDivider();

        foreach (var cat in categories)
        {
            WriteColored($"  {cat.Id,-6}", ConsoleColor.DarkGray);
            WriteLineColored($"{cat.Name,-30}  {cat.TodoItems.Count,-15}", ConsoleColor.Cyan);
        }
        PrintDivider();
    }

    // ──────────────────────────── INPUT HELPERS ────────────────────────────

    public static string ReadLine(string prompt, bool required = false)
    {
        while (true)
        {
            WriteColored($"  {prompt}: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!required || !string.IsNullOrWhiteSpace(input))
                return input;
            WriteError("  Це поле обов'язкове. Спробуйте знову.");
        }
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            WriteColored($"  {prompt}: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (int.TryParse(input, out int result) && result >= min && result <= max)
                return result;
            WriteError($"  Введіть ціле число від {min} до {max}.");
        }
    }

    public static bool Confirm(string prompt)
    {
        WriteColored($"  {prompt} (y/n): ", ConsoleColor.Yellow);
        var input = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
        return input is "y" or "yes" or "т" or "так";
    }

    public static DateTime? ReadDeadline(string prompt)
    {
        while (true)
        {
            WriteColored($"  {prompt} (dd.MM.yyyy HH:mm, порожньо — без дедлайну): ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (DateTime.TryParseExact(input, "dd.MM.yyyy HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                if (dt <= DateTime.Now)
                {
                    WriteError("  Дедлайн не може бути в минулому. Спробуйте знову.");
                    continue;
                }
                return dt;
            }

            WriteError("  Невірний формат. Використовуйте: dd.MM.yyyy HH:mm (наприклад 25.12.2026 18:00)");
        }
    }

    public static Priority ReadPriority()
    {
        WriteLineColored("  Пріоритет:", ConsoleColor.Yellow);
        WriteLineColored("    1. Низький", ConsoleColor.Green);
        WriteLineColored("    2. Середній", ConsoleColor.Yellow);
        WriteLineColored("    3. Високий", ConsoleColor.Red);
        int choice = ReadInt("  Виберіть (1-3)", 1, 3);
        return choice switch
        {
            1 => Priority.Low,
            2 => Priority.Medium,
            3 => Priority.High,
            _ => Priority.Medium
        };
    }

    public static void PressAnyKey()
    {
        Console.WriteLine();
        WriteColored("  Натисніть будь-яку клавішу для продовження...", ConsoleColor.DarkGray);
        Console.ReadKey(true);
        Console.WriteLine();
    }
}
