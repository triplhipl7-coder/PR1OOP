using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FurnitureManagement
{
  
    // 1. ВЛАСНІ КЛАСИ ВИВЯТКІВ (DOMAIN EXCEPTIONS)
  

    // Виняток для помилок валідації даних (порушення форматів)
    public class FurnitureValidationException : Exception
    {
        public FurnitureValidationException(string message) : base(message) { }
    }

    // Виняток для порушення бізнес-правил предметної області
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }


    // 2. ПАТЕРН RESULT ДЛЯ FAULT-TOLERANT ПІДХОДУ
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, T value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
    }

    // Базовий клас сутності
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }

    public static class IdGenerator
    {
        private static int _counter = 0;
        public static int GenerateId() => ++_counter;
    }

    // Статичний модуль перевірок із викиданням власних винятків
    public static class DataValidator
    {
        public static void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
                throw new FurnitureValidationException($"Помилка валідації: Назва виробу \"{title}\" занадто коротка або порожня.");
        }

        public static void ValidatePrice(decimal price)
        {
            if (price <= 0)
                throw new FurnitureValidationException($"Помилка валідації: Ціна {price} не може бути від'ємною або нульовою.");
        }

        public static void ValidateDimensions(double width, double height)
        {
            if (width <= 0 || height <= 0)
                throw new FurnitureValidationException($"Помилка валідації: Габарити ({width}x{height}) мають бути більшими за нуль.");
        }
    }

    // Узагальнений репозиторій з бізнес-обмеженнями
    public class Repository<T> where T : BaseEntity
    {
        private readonly List<T> _items = new List<T>();

        public IEnumerable<T> GetAll() => _items;

        public T GetById(int id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null)
                throw new BusinessRuleException($"Бізнес-помилка: Сутність з ID {id} не знайдена в системі.");
            return item;
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_items.Any(x => x.Id == item.Id))
                throw new BusinessRuleException($"Бізнес-помилка: Елемент з ID {item.Id} вже існує.");
            _items.Add(item);
        }

        public void Update(T item)
        {
            var existing = GetById(item.Id); // Викине бізнес-виняток, якщо немає
            var index = _items.IndexOf(existing);
            _items[index] = item;
        }

        public void Delete(int id)
        {
            var item = GetById(id);
            _items.Remove(item);
        }
    }

    public class FurnitureProject : BaseEntity
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public override string ToString() =>
            $"[ID: {Id}] Виріб: \"{Title}\" | Розміри: {Width}x{Height} мм | Ціна: {Price} грн";
    }

    public class SystemReport
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalItemsCount { get; set; }
        public decimal TotalValue { get; set; }
        public List<FurnitureProject> Items { get; set; }
    }

    // Безпечний сервіс звітності з використанням Result<T> (Fault-tolerant підхід)
    public static class ReportManager
    {
        public static Result<bool> ExportToJson(IEnumerable<FurnitureProject> items, string filePath)
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
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Очікувана проблемна ситуація файлової системи повертається як результат, а не фатальний виняток
                return Result<bool>.Failure($"Попередження файлової системи: Не вдалося записати звіт. Причина: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Repository<FurnitureProject> repository = new Repository<FurnitureProject>();

            Console.WriteLine(" ДЕМОНСТРАЦІЯ ОБРОБКИ ВИНЯТКІВ ТА FAULT-TOLERANT ПІДХОДУ\n");

            // 1. Демонстрація валідації (викидання власного винятку)
            Console.WriteLine("1. Спроба створення виробу з невалідними даними:");
            try
            {
                string badTitle = "Ст"; // Менше 3 символів
                DataValidator.ValidateTitle(badTitle);
            }
            catch (FurnitureValidationException ex)
            {
                Console.WriteLine($"[Спіймано виняток валідації]: {ex.Message}\n");
            }

            // Успішне додавання коректних даних
            try
            {
                string title = "Кухня Лофт Дуб Сонома";
                decimal price = 34500.00m;
                double width = 2800.0;
                double height = 2160.0;

                DataValidator.ValidateTitle(title);
                DataValidator.ValidatePrice(price);
                DataValidator.ValidateDimensions(width, height);

                var project = new FurnitureProject
                {
                    Id = IdGenerator.GenerateId(),
                    Title = title,
                    Price = price,
                    Width = width,
                    Height = height
                };
                repository.Add(project);
                Console.WriteLine($"[Успіх] Додано виріб: {project.Title}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            // 2. Демонстрація бізнес-правила (пошук неіснуючого ID)
            Console.WriteLine("2. Спроба отримати виріб за неіснуючим ID:");
            try
            {
                var nonExistentItem = repository.GetById(999);
            }
            catch (BusinessRuleException ex)
            {
                Console.WriteLine($"[Спіймано бізнес-виняток]: {ex.Message}\n");
            }

            // 3. Демонстрація Fault-tolerant підходу (Result<T> для очікуваної помилки введення/виведення)
            Console.WriteLine("3. Безпечний експорт звіту через патерн Result<T>:");
            // Передаємо некоректний шлях (недозволені символи для шляху файлу, щоб спровокувати I/O помилку)
            string invalidPath = "Z:\\invalid_folder_path\\report.json";

            var exportResult = ReportManager.ExportToJson(repository.GetAll(), invalidPath);

            if (exportResult.IsSuccess)
            {
                Console.WriteLine("[Успіх] Експорт виконано безпомилково.");
            }
            else
            {
                Console.WriteLine($"[М'яка обробка помилки через Result]: {exportResult.ErrorMessage}");
                Console.WriteLine("Програма продовжує роботу стабільно, аварійного завершення не відбулося.");
            }

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }
    }
}
