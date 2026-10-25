using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FurnitureManagement
{

    // 1. ВЛАСНІ КЛАСИ ВИВЯТКІВ (DOMAIN EXCEPTIONS)
  
    public class FurnitureValidationException : Exception
    {
        public FurnitureValidationException(string message) : base(message) { }
    }

    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
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

   
    // 2. МОДУЛЬ ЛОГУВАННЯ ТА ЦЕНТРАЛІЗОВАНОЇ ОБРОБКИ (LOGGER & SAFE EXECUTOR)
   
    public static class Logger
    {
        private static readonly string LogFilePath = "app.log";

        public static void LogError(string errorType, string message, string stackTrace = "")
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{errorType}] {message}";
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    logEntry += $" | StackTrace: {stackTrace.Replace(Environment.NewLine, " -> ")}";
                }

                // Запис до лог-файлу
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Критична помилка логування] Не вдалося записати у файл логів: {ex.Message}");
            }
        }
    }

    public static class OperationHandler
    {
        // Механізм "безпечного виконання" криточних операцій
        public static void SafeExecute(Action operation, string operationName)
        {
            try
            {
                Console.WriteLine($"[Виконання] Запуск операції: \"{operationName}\"...");
                operation();
                Console.WriteLine($"[Успіх] Операція \"{operationName}\" завершена успішно.\n");
            }
            catch (FurnitureValidationException ex)
            {
                Console.WriteLine($"[Перехоплено валідаційний виняток]: {ex.Message}");
                Logger.LogError("VALIDATION_ERROR", ex.Message, ex.StackTrace);
                Console.WriteLine("[Система продовжує роботу стабільно].\n");
            }
            catch (BusinessRuleException ex)
            {
                Console.WriteLine($"[Перехоплено бізнес-виняток]: {ex.Message}");
                Logger.LogError("BUSINESS_RULE_ERROR", ex.Message, ex.StackTrace);
                Console.WriteLine("[Система продовжує роботу стабільно].\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Перехоплено системний виняток]: {ex.Message}");
                Logger.LogError("SYSTEM_CRITICAL_ERROR", ex.Message, ex.StackTrace);
                Console.WriteLine("[Система продовжує роботу стабільно після непередбачуваної помилки].\n");
            }
        }
    }

    // Статичний модуль перевірок
    public static class DataValidator
    {
        public static void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
                throw new FurnitureValidationException($"Назва виробу \"{title}\" занадто коротка або порожня.");
        }

        public static void ValidatePrice(decimal price)
        {
            if (price <= 0)
                throw new FurnitureValidationException($"Ціна {price} не може бути від'ємною або нульовою.");
        }

        public static void ValidateDimensions(double width, double height)
        {
            if (width <= 0 || height <= 0)
                throw new FurnitureValidationException($"Габарити ({width}x{height}) мають бути більшими за нуль.");
        }
    }

    // Узагальнений репозиторій
    public class Repository<T> where T : BaseEntity
    {
        private readonly List<T> _items = new List<T>();

        public IEnumerable<T> GetAll() => _items;

        public T GetById(int id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null)
                throw new BusinessRuleException($"Сутність з ID {id} не знайдена в системі.");
            return item;
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_items.Any(x => x.Id == item.Id))
                throw new BusinessRuleException($"Елемент з ID {item.Id} вже існує.");
            _items.Add(item);
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

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Repository<FurnitureProject> repository = new Repository<FurnitureProject>();

            Console.WriteLine(" ДЕМОНСТРАЦІЯ ЦЕНТРАЛІЗОВАНОЇ ОБРОБКИ ТА ЛОГУВАННЯ \n");

            // Сценарій 1: Успішна операція (додавання коректного виробу)
            OperationHandler.SafeExecute(() =>
            {
                var project = new FurnitureProject
                {
                    Id = IdGenerator.GenerateId(),
                    Title = "Кухня Лофт Дуб Сонома",
                    Price = 34500.00m,
                    Width = 2800.0,
                    Height = 2160.0
                };
                DataValidator.ValidateTitle(project.Title);
                DataValidator.ValidatePrice(project.Price);
                repository.Add(project);
            }, "Додавання коректного меблевого виробу");

            // Сценарій 2: Помилка валідації (занадто коротка назва) — перехопиться і залогується
            OperationHandler.SafeExecute(() =>
            {
                DataValidator.ValidateTitle("Ст"); // Викине FurnitureValidationException
            }, "Валідація назви з помилкою (коротка назва)");

            // Сценарій 3: Порушення бізнес-правила (пошук неіснуючого виробу за ID) — перехопиться і залогується
            OperationHandler.SafeExecute(() =>
            {
                var missingItem = repository.GetById(777); // Викине BusinessRuleException
            }, "Пошук неіснуючого елемента за ID");

            // Виведення вмісту згенерованого лог-файлу для демонстрації звіту
            Console.WriteLine("===");
            Console.WriteLine("ВМІСТ ЗФОРМОВАНОГО ЛОГ-ФАЙЛУ (app.log):");
            Console.WriteLine("===");
            try
            {
                if (File.Exists("app.log"))
                {
                    string logContent = File.ReadAllText("app.log");
                    Console.WriteLine(logContent);
                }
                else
                {
                    Console.WriteLine("Лог-файл ще не створено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося прочитати лог-файл: {ex.Message}");
            }

            Console.WriteLine("Натисніть Enter для виходу...");
            Console.ReadLine();
        }
    }
}
