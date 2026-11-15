using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace FurnitureAnalytics
{
    // описуємо сутність замовлення для нашої предметної області меблів
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // наприклад: Кухні, Шафи, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public bool IsCompleted { get; set; }
    }

    // власна колекція (контейнер сутностей), яка реалізує IEnumerable<Order> для підтримки foreach
    public class FurnitureCollection : IEnumerable<Order>
    {
        private List<Order> _items = new List<Order>();

        // метод додавання нового елемента до нашого контейнера
        public void Add(Order order)
        {
            _items.Add(order);
        }

        // власний метод фільтрації (наприклад, пошук за завершеністю замовлень)
        public FurnitureCollection GetCompletedOrders()
        {
            var filteredCollection = new FurnitureCollection();
            foreach (var order in _items)
            {
                if (order.IsCompleted)
                {
                    filteredCollection.Add(order);
                }
            }
            return filteredCollection;
        }

        // реалізація ітератора для перебору елементів через foreach
        public IEnumerator<Order> GetEnumerator()
        {
            foreach (var order in _items)
            {
                yield return order;
            }
        }

        // негенеридкова версія ітератора для сумісності з інтерфейсами .NET
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // налаштовуємо вивід української мови в консоль без спотворень
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Запуск демонстрації власного контейнера сутностей з підтримкою foreach...");
            Console.WriteLine();

            // створюємо екземпляр нашої власної колекції
            FurnitureCollection repository = new FurnitureCollection();

            // заповнюємо контейнер тестовими даними
            repository.Add(new Order { Id = 1, ClientName = "Олександр Коваленко", Category = "Кухні", TotalPrice = 45000m, AssemblyHours = 8, IsCompleted = true });
            repository.Add(new Order { Id = 2, ClientName = "Марина Шевченко", Category = "Шафи", TotalPrice = 18000m, AssemblyHours = 4, IsCompleted = true });
            repository.Add(new Order { Id = 3, ClientName = "Ігор Бондаренко", Category = "Офісні меблі", TotalPrice = 25000m, AssemblyHours = 5, IsCompleted = false });
            repository.Add(new Order { Id = 4, ClientName = "Світлана Мельник", Category = "Гардеробні", TotalPrice = 34000m, AssemblyHours = 6, IsCompleted = true });
            repository.Add(new Order { Id = 5, ClientName = "Катерина Дмитренко", Category = "Шафи", TotalPrice = 15000m, AssemblyHours = 3, IsCompleted = false });

            Console.WriteLine("1. Перебір усіх елементів ВЛАСНОЇ колекції через цикл foreach:");

            // демонструємо роботу foreach безпосередньо з нашою колекцією
            foreach (var order in repository)
            {
                string status = order.IsCompleted ? "Виконано" : "В процесі";
                Console.WriteLine($"-> Замовлення #{order.Id} | Клієнт: {order.ClientName} | Категорія: {order.Category} | Сума: {order.TotalPrice} грн | Статус: {status}");
            }

            Console.WriteLine();
            Console.WriteLine("2. Бізнес-сценарій: використання вбудованої у контейнер фільтрації (тільки виконані замовлення):");

            // отримуємо відфільтровану власну підколлекцію
            FurnitureCollection completedOrders = repository.GetCompletedOrders();

            decimal totalSum = 0;
            foreach (var order in completedOrders)
            {
                totalSum += order.TotalPrice;
                Console.WriteLine($"-> Виконаний проєкт: {order.ClientName} ({order.Category}) на суму {order.TotalPrice} грн");
            }

            Console.WriteLine();
            Console.WriteLine($"Загальна сума успішно виконаних замовлень у контейнері: {totalSum} грн");

            Console.WriteLine();
            Console.WriteLine("Роботу програми успішно завершено. Натисни будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}