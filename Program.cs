using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // Використовується для роботи з JSON

namespace FurnitureManagement
{
    // Базовий клас для всіх сутностей предметної області
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }

    // Статичний компонент для генерації унікальних ідентифікаторів
    public static class IdGenerator
    {
        private static int _counter = 0;

        public static int GenerateId() => ++_counter;
    }

    // Статичний модуль перевірок коректності даних
    public static class DataValidator
    {
        public static bool ValidateTitle(string title) => !string.IsNullOrWhiteSpace(title) && title.Length >= 3;
        public static bool ValidatePrice(decimal price) => price > 0;
        public static bool ValidateDimensions(double width, double height) => width > 0 && height > 0;
    }

    // Узагальнений клас репозиторію з обмеженням типу (where T : BaseEntity)
    public class Repository<T> where T : BaseEntity
    {
        private readonly List<T> _items = new List<T>();

        public IEnumerable<T> GetAll() => _items;
        public T GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _items.Add(item);
        }

        public void Update(T item)
        {
            var existing = GetById(item.Id);
            if (existing != null)
            {
                var index = _items.IndexOf(existing);
                _items[index] = item;
            }
        }

        public void Delete(int id)
        {
            var item = GetById(id);
            if (item != null) _items.Remove(item);
        }
    }

    // Конкретна сутність предметної області — меблевий виріб
    public class FurnitureProject : BaseEntity
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public override string ToString() =>
            $"[ID: {Id}] Виріб: \"{Title}\" | Розміри: {Width}x{Height} мм | Ціна: {Price} грн";
    }

    // Клас для структури звіту (для зручності серіалізації)
    public class SystemReport
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalItemsCount { get; set; }
        public decimal TotalValue { get; set; }
        public List<FurnitureProject> Items { get; set; }
    }

    // Модуль формування звітності та експорту з обробкою помилок I/O
    public static class ReportManager
    {
        public static void ExportToJson(IEnumerable<FurnitureProject> items, string filePath)
        {
            try
            {
                var report = new SystemReport
                {
                    GeneratedAt = DateTime.Now,
                    TotalItemsCount = items.Count(),
                    TotalValue = items.Sum(x => x.Price),
                    Items = items.ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(report, options);

                File.WriteAllText(filePath, jsonString);
                Console.WriteLine($"[Успіх] Звіт успішно експортовано у файл: {filePath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("[Помилка I/O] Немає прав доступу для запису файлу у вказану директорію.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("[Помилка I/O] Вказаний шлях до директорії не знайдено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Помилка] Сталася непередбачена помилка під час запису: {ex.Message}");
            }
        }

        public static void ImportAndVerifyJson(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("[Попередження] Файл звіту не знайдено для перевірки.");
                    return;
                }

                string jsonString = File.ReadAllText(filePath);
                var report = JsonSerializer.Deserialize<SystemReport>(jsonString);

                Console.WriteLine("\n--- ПЕРЕВІРКА ЗЧИТАНОГО ФАЙЛУ ЗВІТУ ---");
                Console.WriteLine($"Дата створення звіту: {report.GeneratedAt}");
                Console.WriteLine($"Загальна кількість виробів у звіті: {report.TotalItemsCount}");
                Console.WriteLine($"Загальна вартість: {report.TotalValue} грн");
                Console.WriteLine("Перелік зчитаних сутностей:");
                foreach (var item in report.Items)
                {
                    Console.WriteLine($" - {item.Title} ({item.Price} грн)");
                }
                Console.WriteLine("---------------------------------------");
            }
            catch (JsonException)
            {
                Console.WriteLine("[Помилка формату] Пошкоджена структура JSON-файлу під час зчитування.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Помилка I/O] Не вдалося прочитати файл: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Repository<FurnitureProject> repository = new Repository<FurnitureProject>();

            Console.WriteLine("=== УПРАВЛІННЯ ПРОЄКТАМИ МЕБЛЕВОГО ВИРОБНИЦТВА ТА ЗВІТНІСТЬ ===\n");

            // Додаємо кілька тестових виробів
            var itemsToAdd = new[]
            {
                (Title: "Кухня Лофт Дуб Сонома", Price: 34500.00m, Width: 2800.0, Height: 2160.0),
                (Title: "Шафа-купе Дзеркальна", Price: 18200.50m, Width: 2000.0, Height: 2400.0),
                (Title: "Офісна стійка ресепшн", Price: 14000.00m, Width: 1800.0, Height: 1100.0)
            };

            foreach (var data in itemsToAdd)
            {
                if (DataValidator.ValidateTitle(data.Title) &&
                    DataValidator.ValidatePrice(data.Price) &&
                    DataValidator.ValidateDimensions(data.Width, data.Height))
                {
                    repository.Add(new FurnitureProject
                    {
                        Id = IdGenerator.GenerateId(),
                        Title = data.Title,
                        Price = data.Price,
                        Width = data.Width,
                        Height = data.Height
                    });
                }
            }

            // Шлях до файлу звіту
            string reportPath = "furniture_report.json";

            // Експорт даних у файл з обробкою помилок
            ReportManager.ExportToJson(repository.GetAll(), reportPath);

            // Повторне зчитування та перевірка коректності файлу
            ReportManager.ImportAndVerifyJson(reportPath);

            // Демонстрація реакції на помилкову ситуацію (спроба зчитати неіснуючий файл)
            Console.WriteLine("\n[Тест обробки помилок] Спроба відкрити неіснуючий файл:");
            ReportManager.ImportAndVerifyJson("non_existent_file.json");

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }
    }
}
