using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FurnitureAnalytics
{
    // тут описуємо структуру одного замовлення, щоб зберігати всю необхідну інфу
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // наприклад: Кухні, Шафи, Офісні меблі, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // налаштовуємо вивід української мови в консоль, щоб нічого не кодилося кракозябрами
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Запуск генерації аналітичного звіту за допомогою LINQ...");

            // створюємо початковий масив тестових даних по меблях
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

            // сюди складатимемо текст нашого звіту, щоб потім вивести його і одразу записати у файл
            List<string> reportLines = new List<string>();

            reportLines.Add("КОМПЛЕКСНИЙ АНАЛІТИЧНИЙ ЗВІТ ПО ЗАМОВЛЕННЯХ");
            reportLines.Add($"Дата і час формування: {DateTime.Now:yyyy-MM-dd HH:mm}");
            reportLines.Add($"Всього замовлень зафіксовано в системі: {orders.Count()}");
            reportLines.Add("");

            // рахуємо загальні агрегатні показники по всій базі даних
            decimal totalRevenue = orders.Where(o => o.IsCompleted).Sum(o => o.TotalPrice);
            decimal avgPrice = orders.Average(o => o.TotalPrice);
            decimal maxPrice = orders.Max(o => o.TotalPrice);
            decimal minPrice = orders.Min(o => o.TotalPrice);

            reportLines.Add("Загальні фінансові та кількісні метрики:");
            reportLines.Add($"Загальна виручка по виконаних проєктах: {totalRevenue:N0} грн");
            reportLines.Add($"Середня вартість одного замовлення: {avgPrice:N2} грн");
            reportLines.Add($"Найдорожчий виконаний проєкт: {maxPrice:N0} грн");
            reportLines.Add($"Найнижча вартість замовлення: {minPrice:N0} грн");
            reportLines.Add("");

            // робимо групування за категоріями меблів і рахуємо статистику для кожної групи окремо
            reportLines.Add("Аналіз за категоріями меблів (групування та агрегація):");

            var categoryReport = orders
                .GroupBy(o => o.Category)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    OrderCount = g.Count(),
                    CompletedCount = g.Count(o => o.IsCompleted),
                    TotalCategoryRevenue = g.Where(o => o.IsCompleted).Sum(o => o.TotalPrice),
                    AverageAssemblyHours = g.Average(o => o.AssemblyHours),
                    MaxCategoryPrice = g.Max(o => o.TotalPrice)
                })
                .OrderByDescending(r => r.TotalCategoryRevenue);

            foreach (var item in categoryReport)
            {
                reportLines.Add($"Категорія: {item.CategoryName}");
                reportLines.Add($"  -> всього замовлень: {item.OrderCount}, з них виконано: {item.CompletedCount}");
                reportLines.Add($"  -> прибуток по категорії: {item.TotalCategoryRevenue:N0} грн");
                reportLines.Add($"  -> середній час на збирання: {item.AverageAssemblyHours:N1} год");
                reportLines.Add($"  -> максимальний чек у цій категорії: {item.MaxCategoryPrice:N0} грн");
                reportLines.Add("");
            }

            // виводимо звіт на екран консолі рядок за рядком
            foreach (var line in reportLines)
            {
                Console.WriteLine(line);
            }

            // зберігаємо сформований звіт у текстовий файл у папці з проєктом
            string fileName = "analytical_report.txt";
            File.WriteAllLines(fileName, reportLines);
            Console.WriteLine($"Файл успішно збережено на диск за шляхом: {Path.GetFullPath(fileName)}");

            Console.WriteLine("Роботу програми завершено. Натисни будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}