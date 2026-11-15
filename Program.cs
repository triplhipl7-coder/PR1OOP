using System;
using System.Collections.Generic;
using System.Linq;

namespace FurnitureAnalytics
{
    // 1. Доменна модель
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // Кухні, Шафи, Офісні меблі, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsCompleted { get; set; }
    }
    class Program

    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(" ПІДСИСТЕМА АНАЛІТИЧНИХ ЗАПИТІВ LINQ ");

            // 2. Набір даних доменної моделі
            List<Order> orders = new List<Order>
            {
                new Order { Id = 1, ClientName = "Олександр Коваленко", Category = "Кухні", TotalPrice = 45000m, AssemblyHours = 8, OrderDate = new DateTime(2026, 10, 5), IsCompleted = true },
                new Order { Id = 2, ClientName = "Марина Шевченко", Category = "Шафи", TotalPrice = 18000m, AssemblyHours = 4, OrderDate = new DateTime(2026, 10, 10), IsCompleted = true },
                new Order { Id = 3, ClientName = "Ігор Бондаренко", Category = "Офісні меблі", TotalPrice = 25000m, AssemblyHours = 5, OrderDate = new DateTime(2026, 10, 12), IsCompleted = false },
                new Order { Id = 4, ClientName = "Світлана Мельник", Category = "Кухні", TotalPrice = 62000m, AssemblyHours = 10, OrderDate = new DateTime(2026, 10, 15), IsCompleted = true },
                new Order { Id = 5, ClientName = "Андрій Шевчук", Category = "Гардеробні", TotalPrice = 34000m, AssemblyHours = 6, OrderDate = new DateTime(2026, 11, 1), IsCompleted = true },
                new Order { Id = 6, ClientName = "Катерина Дмитренко", Category = "Шафи", TotalPrice = 15000m, AssemblyHours = 3, OrderDate = new DateTime(2026, 11, 3), IsCompleted = false },
                new Order { Id = 7, ClientName = "Юрій Павленко", Category = "Офісні меблі", TotalPrice = 41000m, AssemblyHours = 7, OrderDate = new DateTime(2026, 11, 5), IsCompleted = true },
                new Order { Id = 8, ClientName = "Тетяна Романюк", Category = "Кухні", TotalPrice = 53000m, AssemblyHours = 9, OrderDate = new DateTime(2026, 11, 8), IsCompleted = true }
            };

            // --- ЗАПИТ 1: Фільтрація та сортування (Завершені замовлення дорожчі за 30000, сортування за спаданням ціни) ---
            Console.WriteLine("\n ЗАПИТ 1: Фільтрація та сортування (Ціна > 30000, тільки завершені) ");
            var expensiveCompletedOrders = orders
                .Where(o => o.IsCompleted && o.TotalPrice > 30000m)
                .OrderByDescending(o => o.TotalPrice);

            foreach (var order in expensiveCompletedOrders)
            {
                Console.WriteLine($"Клієнт: {order.ClientName,-20} | Категорія: {order.Category,-12} | Сума: {order.TotalPrice,8} грн");
            }

            //  ЗАПИТ 2 Проєкція (Вибірка анонімного типу з прізвищем клієнта та розрахунком вартості години роботи) 
            Console.WriteLine("\n ЗАПИТ 2  Проєкція (Розрахунок ефективності: грн/год збирання) ");
            var orderEfficiency = orders
                .Select(o => new
                {
                    o.ClientName,
                    o.Category,
                    HourlyRate = Math.Round(o.TotalPrice / o.AssemblyHours, 2)
                });

            foreach (var item in orderEfficiency)
            {
                Console.WriteLine($"Клієнт: {item.ClientName,-20} | Категорія: {item.Category,-12} | Дохід за год: {item.HourlyRate,8} грн/год");
            }

            //  ЗАПИТ 3: Агрегування (Загальна статистика по всім замовленням) 
            Console.WriteLine("\n ЗАПИТ 3: Агрегування (Загальні фінансові та часові показники) ");
            decimal totalRevenue = orders.Where(o => o.IsCompleted).Sum(o => o.TotalPrice);
            decimal averageOrderPrice = orders.Average(o => o.TotalPrice);
            int maxAssemblyHours = orders.Max(o => o.AssemblyHours);
            int totalOrdersCount = orders.Count();

            Console.WriteLine($"Всього замовлень у системі: {totalOrdersCount}");
            Console.WriteLine($"Загальна виручка по виконаних: {totalRevenue} грн");
            Console.WriteLine($"Середній чек замовлення: {Math.Round(averageOrderPrice, 2)} грн");
            Console.WriteLine($"Максимальний час збирання одного проєкту: {maxAssemblyHours} годин");

            //  ЗАПИТ 4: Групування з формуванням підсумкової статистики за категоріями 
            Console.WriteLine("\n ЗАПИТ 4: Групування за категоріями меблів з підсумками ");
            var categoryGroupQuery = orders
                .GroupBy(o => o.Category)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice),
                    AverageHours = Math.Round(g.Average(o => o.AssemblyHours), 1)
                });

            foreach (var group in categoryGroupQuery)
            {
                Console.WriteLine($"Категорія: {group.CategoryName,-15} | Кількість: {group.Count} | Сума: {group.TotalRevenue,8} грн | Сер. час: {group.AverageHours} год");
            }

            //  ЗАПИТ 5: Комплексний запит (Пошук клієнтів за ключовим словом та групування/сортування) ---
            Console.WriteLine("\n ЗАПИТ 5: Пошук та аналіз незавершених замовлень");
            var pendingOrders = from o in orders
                                where !o.IsCompleted
                                orderby o.OrderDate
                                select new { o.ClientName, o.Category, o.OrderDate };

            foreach (var p in pendingOrders)
            {
                Console.WriteLine($"Очікує виконання: {p.ClientName,-20} | {p.Category,-12} | Дата: {p.OrderDate.ToShortDateString()}");
            }

            //  ДЕМОНСТРАЦІЯ: Відкладене виконання (Deferred Execution) проти Матеріалізації (Immediate Execution) 
            Console.WriteLine("\n ДЕМОНСТРАЦІЯ РІЗНИЦІ МІЖ ВІДКЛАДЕНИМ ВИКОНАННЯМ ТА МАТЕРІАЛІЗАЦІЄЮ ");

            // 1. Відкладений запит (Query Syntax / Method Syntax)
            // Запит НЕ виконується в момент оголошення, він лише зберігає правило.
            var deferredQuery = orders.Where(o => o.TotalPrice > 40000m);
            Console.WriteLine("[Deferred] Запит створено. Додаємо нове дороге замовлення в колекцію...");

            // Додаємо новий елемент у вихідну колекцію ПСИХОЛОГІЧНО ПІСЛЯ створення запиту
            orders.Add(new Order { Id = 9, ClientName = "Віктор Назаренко", Category = "Кухні", TotalPrice = 75000m, AssemblyHours = 12, OrderDate = DateTime.Now, IsCompleted = true });

            Console.WriteLine($"[Deferred] Виводимо результати відкладеного запиту (кількість елементів: {deferredQuery.Count()}):");
            foreach (var item in deferredQuery)
            {
                Console.WriteLine($"-> {item.ClientName} ({item.TotalPrice} грн) — враховано новий елемент завдяки відкладеному виконанню!");
            }

            // 2. Матеріалізований запит (.ToList() / .ToArray())
            // Матеріалізація виконується негайно і фіксує стан даних на момент виклику.
            var immediateList = orders.Where(o => o.TotalPrice > 60000m).ToList();
            Console.WriteLine("\n[Immediate] Запит матеріалізовано (.ToList()). Змінюємо дані в колекції...");

            // Додаємо ще одне замовлення
            orders.Add(new Order { Id = 10, ClientName = "Олена Коваль", Category = "Гардеробні", TotalPrice = 90000m, AssemblyHours = 14, OrderDate = DateTime.Now, IsCompleted = true });

            Console.WriteLine($"[Immediate] Виводимо матеріалізований список (кількість елементів: {immediateList.Count}):");
            foreach (var item in immediateList)
            {
                Console.WriteLine($"-> {item.ClientName} ({item.TotalPrice} грн) — нове замовлення НЕ потрапило сюди, бо список вже був матеріалізований!");
            }

            Console.WriteLine("\nРоботу програми завершено. Натисніть будь-яку клавішу для виходу.");
            Console.ReadKey();
        }
    }
}