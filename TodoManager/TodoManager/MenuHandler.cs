using TodoManager.Models;
using TodoManager.Services;

namespace TodoManager;

public class MenuHandler
{
    private readonly TodoService _service;

    public MenuHandler(TodoService service)
    {
        _service = service;
    }

    // ══════════════════════════ MAIN MENU ══════════════════════════

    public async Task RunAsync()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            ConsoleHelper.PrintTitle("TODO-МЕНЕДЖЕР");

            ConsoleHelper.WriteLineColored("  1. ➕  Додати завдання", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  2. 📋  Переглянути всі завдання", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  3. ✓   Позначити виконаним / невиконаним", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  4. ✏️  Редагувати завдання", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  5. 🗑️  Видалити завдання", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  6. 🔍  Фільтри", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  7. 📁  Категорії", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  8. 📊  Статистика", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  0. 🚪  Вихід", ConsoleColor.DarkGray);
            Console.WriteLine();

            ConsoleHelper.WriteColored("  Ваш вибір: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            switch (input)
            {
                case "1": await AddTodoAsync(); break;
                case "2": await ViewAllTodosAsync(); break;
                case "3": await ToggleDoneAsync(); break;
                case "4": await EditTodoAsync(); break;
                case "5": await DeleteTodoAsync(); break;
                case "6": await FiltersMenuAsync(); break;
                case "7": await CategoriesMenuAsync(); break;
                case "8": await ShowStatisticsAsync(); break;
                case "0":
                    ConsoleHelper.WriteSuccess("\n  До побачення! 👋");
                    return;
                default:
                    ConsoleHelper.WriteError("  Невірний вибір. Введіть число від 0 до 8.");
                    ConsoleHelper.PressAnyKey();
                    break;
            }
        }
    }

    // ══════════════════════════ ADD TODO ══════════════════════════

    private async Task AddTodoAsync()
    {
        ConsoleHelper.PrintTitle("ДОДАТИ ЗАВДАННЯ");

        // Title
        string title = ConsoleHelper.ReadLine("Назва завдання", required: true);

        // Description
        string description = ConsoleHelper.ReadLine("Опис (необов'язково)");

        // Deadline
        DateTime? deadline = ConsoleHelper.ReadDeadline("Дедлайн");

        // Priority
        Priority priority = ConsoleHelper.ReadPriority();

        // Category
        int categoryId = await SelectOrCreateCategoryAsync();
        if (categoryId <= 0)
        {
            ConsoleHelper.WriteError("  Скасовано — категорія не вибрана.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        var item = await _service.CreateTodoAsync(title, description, deadline, priority, categoryId);
        ConsoleHelper.WriteSuccess($"\n  ✓ Завдання #{item.Id} «{item.Title}» успішно додано!");
        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ VIEW ALL ══════════════════════════

    private async Task ViewAllTodosAsync()
    {
        ConsoleHelper.PrintTitle("УСІ ЗАВДАННЯ");
        var items = await _service.GetAllTodosAsync();
        ConsoleHelper.PrintTodoTable(items);
        PrintLegend();
        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ TOGGLE DONE ══════════════════════════

    private async Task ToggleDoneAsync()
    {
        ConsoleHelper.PrintTitle("ПОЗНАЧИТИ ВИКОНАНИМ / НЕВИКОНАНИМ");
        var items = await _service.GetAllTodosAsync();
        ConsoleHelper.PrintTodoTable(items);

        if (items.Count == 0) { ConsoleHelper.PressAnyKey(); return; }

        int id = ConsoleHelper.ReadInt("\n  Введіть ID завдання", 1);
        var result = await _service.ToggleDoneAsync(id);

        if (result)
        {
            var updated = await _service.GetTodoByIdAsync(id);
            string status = updated!.IsDone ? "виконане ✓" : "невиконане";
            ConsoleHelper.WriteSuccess($"  ✓ Завдання #{id} тепер позначене як {status}.");
        }
        else
        {
            ConsoleHelper.WriteError($"  ✗ Завдання з ID {id} не знайдено.");
        }
        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ EDIT TODO ══════════════════════════

    private async Task EditTodoAsync()
    {
        ConsoleHelper.PrintTitle("РЕДАГУВАТИ ЗАВДАННЯ");
        var items = await _service.GetAllTodosAsync();
        ConsoleHelper.PrintTodoTable(items);

        if (items.Count == 0) { ConsoleHelper.PressAnyKey(); return; }

        int id = ConsoleHelper.ReadInt("\n  Введіть ID завдання для редагування", 1);
        var existing = await _service.GetTodoByIdAsync(id);

        if (existing == null)
        {
            ConsoleHelper.WriteError($"  ✗ Завдання з ID {id} не знайдено.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        ConsoleHelper.WriteInfo($"\n  Поточні дані завдання #{id}:");
        ConsoleHelper.WriteInfo($"  Назва: {existing.Title}");
        ConsoleHelper.WriteInfo($"  Опис: {existing.Description}");
        ConsoleHelper.WriteInfo($"  Дедлайн: {(existing.Deadline.HasValue ? existing.Deadline.Value.ToString("dd.MM.yyyy HH:mm") : "—")}");
        ConsoleHelper.WriteInfo($"  Пріоритет: {PriorityName(existing.Priority)}");
        ConsoleHelper.WriteInfo($"  Категорія: {existing.Category?.Name}");
        Console.WriteLine();
        ConsoleHelper.WriteWarning("  (Залиште поле порожнім, щоб не змінювати значення)");
        Console.WriteLine();

        // Title
        ConsoleHelper.WriteColored($"  Нова назва [{existing.Title}]: ", ConsoleColor.Yellow);
        string newTitle = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newTitle)) newTitle = existing.Title;

        // Description
        ConsoleHelper.WriteColored($"  Новий опис [{existing.Description}]: ", ConsoleColor.Yellow);
        string newDesc = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newDesc)) newDesc = existing.Description;

        // Deadline
        ConsoleHelper.WriteColored("  Змінити дедлайн? (y/n): ", ConsoleColor.Yellow);
        DateTime? newDeadline = existing.Deadline;
        if (Console.ReadLine()?.Trim().ToLower() == "y")
            newDeadline = ConsoleHelper.ReadDeadline("Новий дедлайн");

        // Priority
        ConsoleHelper.WriteColored("  Змінити пріоритет? (y/n): ", ConsoleColor.Yellow);
        Priority newPriority = existing.Priority;
        if (Console.ReadLine()?.Trim().ToLower() == "y")
            newPriority = ConsoleHelper.ReadPriority();

        // Category
        ConsoleHelper.WriteColored("  Змінити категорію? (y/n): ", ConsoleColor.Yellow);
        int newCategoryId = existing.CategoryId;
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            int selectedId = await SelectOrCreateCategoryAsync();
            if (selectedId > 0) newCategoryId = selectedId;
        }

        var success = await _service.UpdateTodoAsync(id, newTitle, newDesc, newDeadline, newPriority, newCategoryId);
        if (success)
            ConsoleHelper.WriteSuccess($"\n  ✓ Завдання #{id} успішно оновлено!");
        else
            ConsoleHelper.WriteError($"  ✗ Не вдалося оновити завдання #{id}.");

        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ DELETE TODO ══════════════════════════

    private async Task DeleteTodoAsync()
    {
        ConsoleHelper.PrintTitle("ВИДАЛИТИ ЗАВДАННЯ");
        var items = await _service.GetAllTodosAsync();
        ConsoleHelper.PrintTodoTable(items);

        if (items.Count == 0) { ConsoleHelper.PressAnyKey(); return; }

        int id = ConsoleHelper.ReadInt("\n  Введіть ID завдання для видалення", 1);
        var existing = await _service.GetTodoByIdAsync(id);

        if (existing == null)
        {
            ConsoleHelper.WriteError($"  ✗ Завдання з ID {id} не знайдено.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        ConsoleHelper.WriteWarning($"\n  Ви збираєтесь видалити: «{existing.Title}»");
        bool confirmed = ConsoleHelper.Confirm("  Підтвердіть видалення");

        if (!confirmed)
        {
            ConsoleHelper.WriteInfo("  Скасовано.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        var success = await _service.DeleteTodoAsync(id);
        if (success)
            ConsoleHelper.WriteSuccess($"  ✓ Завдання #{id} «{existing.Title}» видалено.");
        else
            ConsoleHelper.WriteError($"  ✗ Помилка при видаленні завдання #{id}.");

        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ FILTERS MENU ══════════════════════════

    private async Task FiltersMenuAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintTitle("ФІЛЬТРИ");
            ConsoleHelper.WriteLineColored("  1. Тільки невиконані", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  2. Тільки з пріоритетом «Високий»", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  3. Прострочені (дедлайн минув, не виконано)", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  0. Назад", ConsoleColor.DarkGray);
            Console.WriteLine();

            ConsoleHelper.WriteColored("  Ваш вибір: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            switch (input)
            {
                case "1":
                    ConsoleHelper.PrintTitle("НЕВИКОНАНІ ЗАВДАННЯ");
                    var pending = await _service.GetPendingAsync();
                    ConsoleHelper.PrintTodoTable(pending);
                    PrintLegend();
                    ConsoleHelper.PressAnyKey();
                    break;

                case "2":
                    ConsoleHelper.PrintTitle("ЗАВДАННЯ З ВИСОКИМ ПРІОРИТЕТОМ");
                    var highPriority = await _service.GetHighPriorityAsync();
                    ConsoleHelper.PrintTodoTable(highPriority);
                    PrintLegend();
                    ConsoleHelper.PressAnyKey();
                    break;

                case "3":
                    ConsoleHelper.PrintTitle("ПРОСТРОЧЕНІ ЗАВДАННЯ");
                    var overdue = await _service.GetOverdueAsync();
                    ConsoleHelper.PrintTodoTable(overdue);
                    PrintLegend();
                    ConsoleHelper.PressAnyKey();
                    break;

                case "0":
                    return;

                default:
                    ConsoleHelper.WriteError("  Невірний вибір.");
                    ConsoleHelper.PressAnyKey();
                    break;
            }
        }
    }

    // ══════════════════════════ CATEGORIES MENU ══════════════════════════

    private async Task CategoriesMenuAsync()
    {
        while (true)
        {
            ConsoleHelper.PrintTitle("КАТЕГОРІЇ");
            ConsoleHelper.WriteLineColored("  1. Переглянути всі категорії", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  2. Створити нову категорію", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  3. Переглянути завдання категорії", ConsoleColor.White);
            ConsoleHelper.WriteLineColored("  0. Назад", ConsoleColor.DarkGray);
            Console.WriteLine();

            ConsoleHelper.WriteColored("  Ваш вибір: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            switch (input)
            {
                case "1":
                    ConsoleHelper.PrintTitle("УСІ КАТЕГОРІЇ");
                    var cats = await _service.GetAllCategoriesAsync();
                    // Load todos count
                    foreach (var c in cats)
                        c.TodoItems = (await _service.GetByCategoryAsync(c.Id));
                    ConsoleHelper.PrintCategoryTable(cats);
                    ConsoleHelper.PressAnyKey();
                    break;

                case "2":
                    ConsoleHelper.PrintTitle("НОВА КАТЕГОРІЯ");
                    string name = ConsoleHelper.ReadLine("Назва категорії", required: true);
                    var existing = await _service.GetCategoryByNameAsync(name);
                    if (existing != null)
                    {
                        ConsoleHelper.WriteWarning($"  Категорія «{name}» вже існує (ID: {existing.Id}).");
                    }
                    else
                    {
                        var cat = await _service.CreateCategoryAsync(name);
                        ConsoleHelper.WriteSuccess($"  ✓ Категорія «{cat.Name}» (ID: {cat.Id}) успішно створена!");
                    }
                    ConsoleHelper.PressAnyKey();
                    break;

                case "3":
                    await ViewByCategoryAsync();
                    break;

                case "0":
                    return;

                default:
                    ConsoleHelper.WriteError("  Невірний вибір.");
                    ConsoleHelper.PressAnyKey();
                    break;
            }
        }
    }

    private async Task ViewByCategoryAsync()
    {
        ConsoleHelper.PrintTitle("ЗАВДАННЯ ЗА КАТЕГОРІЄЮ");
        var categories = await _service.GetAllCategoriesAsync();

        if (categories.Count == 0)
        {
            ConsoleHelper.WriteWarning("  Категорій ще немає. Спочатку створіть категорію.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        foreach (var c in categories)
            ConsoleHelper.WriteLineColored($"  [{c.Id}] {c.Name}", ConsoleColor.Cyan);

        int catId = ConsoleHelper.ReadInt("\n  Введіть ID категорії", 1);
        var category = await _service.GetCategoryByIdAsync(catId);

        if (category == null)
        {
            ConsoleHelper.WriteError($"  Категорія з ID {catId} не знайдена.");
            ConsoleHelper.PressAnyKey();
            return;
        }

        ConsoleHelper.PrintTitle($"ЗАВДАННЯ КАТЕГОРІЇ «{category.Name.ToUpper()}»");
        var items = await _service.GetByCategoryAsync(catId);
        ConsoleHelper.PrintTodoTable(items);
        PrintLegend();
        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ STATISTICS ══════════════════════════

    private async Task ShowStatisticsAsync()
    {
        ConsoleHelper.PrintTitle("📊 СТАТИСТИКА");

        var (total, done, pending, overdue, completedThisWeek) = await _service.GetStatsAsync();

        ConsoleHelper.PrintDivider();
        ConsoleHelper.WriteColored("  Всього завдань:               ", ConsoleColor.Gray);
        ConsoleHelper.WriteLineColored($"{total}", ConsoleColor.White);

        ConsoleHelper.WriteColored("  Виконано:                     ", ConsoleColor.Gray);
        ConsoleHelper.WriteLineColored($"{done}", ConsoleColor.Green);

        ConsoleHelper.WriteColored("  Невиконано:                   ", ConsoleColor.Gray);
        ConsoleHelper.WriteLineColored($"{pending}", ConsoleColor.Yellow);

        ConsoleHelper.WriteColored("  Прострочено:                  ", ConsoleColor.Gray);
        ConsoleHelper.WriteLineColored($"{overdue}", ConsoleColor.Red);

        ConsoleHelper.WriteColored("  Виконано за останній тиждень: ", ConsoleColor.Gray);
        ConsoleHelper.WriteLineColored($"{completedThisWeek}", ConsoleColor.Cyan);

        if (total > 0)
        {
            int percent = (int)((double)done / total * 100);
            ConsoleHelper.PrintDivider();
            ConsoleHelper.WriteColored("  Прогрес: ", ConsoleColor.Gray);

            int filled = percent / 5;
            string bar = "[" + new string('█', filled) + new string('░', 20 - filled) + "]";
            ConsoleHelper.WriteColored($"{bar} {percent}%", percent >= 70 ? ConsoleColor.Green : ConsoleColor.Yellow);
            Console.WriteLine();
        }

        ConsoleHelper.PrintDivider();
        ConsoleHelper.PressAnyKey();
    }

    // ══════════════════════════ HELPERS ══════════════════════════

    private async Task<int> SelectOrCreateCategoryAsync()
    {
        var categories = await _service.GetAllCategoriesAsync();

        ConsoleHelper.PrintSectionTitle("КАТЕГОРІЯ");

        if (categories.Count > 0)
        {
            foreach (var c in categories)
                ConsoleHelper.WriteLineColored($"  [{c.Id}] {c.Name}", ConsoleColor.Cyan);

            ConsoleHelper.WriteLineColored("  [0] Створити нову категорію", ConsoleColor.DarkGray);
            Console.WriteLine();

            ConsoleHelper.WriteColored("  Виберіть ID категорії або 0 для нової: ", ConsoleColor.Yellow);
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (int.TryParse(input, out int selectedId) && selectedId > 0)
            {
                var found = categories.FirstOrDefault(c => c.Id == selectedId);
                if (found != null) return found.Id;
                ConsoleHelper.WriteError($"  Категорія {selectedId} не знайдена.");
            }
        }
        else
        {
            ConsoleHelper.WriteWarning("  Категорій ще немає. Потрібно створити першу.");
        }

        // Create new category
        string newName = ConsoleHelper.ReadLine("  Назва нової категорії", required: true);
        var existing = await _service.GetCategoryByNameAsync(newName);
        if (existing != null)
        {
            ConsoleHelper.WriteInfo($"  Використано існуючу категорію «{existing.Name}».");
            return existing.Id;
        }

        var created = await _service.CreateCategoryAsync(newName);
        ConsoleHelper.WriteSuccess($"  ✓ Категорія «{created.Name}» створена.");
        return created.Id;
    }

    private static string PriorityName(Priority p) => p switch
    {
        Priority.High => "Високий",
        Priority.Medium => "Середній",
        Priority.Low => "Низький",
        _ => "?"
    };

    private static void PrintLegend()
    {
        Console.WriteLine();
        ConsoleHelper.WriteColored("  Легенда: ", ConsoleColor.DarkGray);
        ConsoleHelper.WriteColored("[ ] невиконане  ", ConsoleColor.White);
        ConsoleHelper.WriteColored("[✓] виконане  ", ConsoleColor.Green);
        ConsoleHelper.WriteColored("[!] прострочене", ConsoleColor.Red);
        Console.WriteLine();
    }
}
