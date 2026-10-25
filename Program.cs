using System;
using System.Collections.Generic;
using System.Linq;

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

        public static int GenerateId()
        {
            return ++_counter;
        }
    }

    // Статичний модуль перевірок коректності даних (валідації)
    public static class DataValidator
    {
        public static bool ValidateTitle(string title)
        {
            return !string.IsNullOrWhiteSpace(title) && title.Length >= 3;
        }

        public static bool ValidatePrice(decimal price)
        {
            return price > 0;
        }

        public static bool ValidateDimensions(double width, double height)
        {
            return width > 0 && height > 0;
        }
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
            if (item != null)
            {
                _items.Remove(item);
            }
        }
    }

    // Конкретна сутність предметної області — меблевий виріб
    public class FurnitureProject : BaseEntity
    {
        public string Title { get; set; }        // Назва проєкту / виробу
        public decimal Price { get; set; }       // Вартість (грн)
        public double Width { get; set; }        // Ширина (мм)
        public double Height { get; set; }       // Висота (мм)

        public override string ToString() =>
            $"[ID: {Id}] Виріб: \"{Title}\" | Розміри: {Width}x{Height} мм | Ціна: {Price} грн";
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного відображення української мови в консолі
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Ініціалізація узагальненого репозиторію для сутності FurnitureProject
            Repository<FurnitureProject> repository = new Repository<FurnitureProject>();

            Console.WriteLine(" УПРАВЛІННЯ ПРОЄКТАМИ МЕБЛЕВОГО ВИРОБНИЦТВА \n");

            // Дані для додавання (використовуємо реальні назви меблів)
            var itemsToAdd = new[]
            {
                (Title: "Кухня Лофт Дуб Сонома", Price: 34500.00m, Width: 2800.0, Height: 2160.0),
                (Title: "Шафа-купе Дзеркальна", Price: 18200.50m, Width: 2000.0, Height: 2400.0),
                (Title: "Офісна стійка ресепшн", Price: 14000.00m, Width: 1800.0, Height: 1100.0),
                (Title: "Підвісна тумба під ТВ", Price: 6500.00m, Width: 1600.0, Height: 3500.0) // Приклад з помилкою у висоті для перевірки валідатора
            };

            // Додавання елементів через валідатор та генератор ID
            foreach (var data in itemsToAdd)
            {
                if (DataValidator.ValidateTitle(data.Title) &&
                    DataValidator.ValidatePrice(data.Price) &&
                    DataValidator.ValidateDimensions(data.Width, data.Height))
                {
                    var project = new FurnitureProject
                    {
                        Id = IdGenerator.GenerateId(),
                        Title = data.Title,
                        Price = data.Price,
                        Width = data.Width,
                        Height = data.Height
                    };

                    repository.Add(project);
                    Console.WriteLine($"[Успіх] Додано сутність: {project.Title}");
                }
                else
                {
                    Console.WriteLine($"[Помилка валідації] Неможливо додати виріб \"{data.Title}\" через некоректні дані.");
                }
            }

            // Виведення списку всіх збережених сутностей через репозиторій
            Console.WriteLine("\n Поточний список елементів у репозиторії ");
            foreach (var item in repository.GetAll())
            {
                Console.WriteLine(item);
            }

            // Демонстрація оновлення запису
            Console.WriteLine("\n Оновлення даних виробу з ID 1 ");
            var itemToUpdate = repository.GetById(1);
            if (itemToUpdate != null)
            {
                itemToUpdate.Price = 36000.00m; // Зміна ціни
                repository.Update(itemToUpdate);
                Console.WriteLine($"Оновлено успішно: {repository.GetById(1)}");
            }

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }
    }
}
