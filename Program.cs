using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace FurnitureAnalytics
{
    // структура одного замовлення у нашій предметній області
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // наприклад: Кухні, Шафи, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public bool IsCompleted { get; set; }
    }

    // інтерфейс для контрактів збереження даних (зменшує зв'язаність коду)
    public interface IOrderRepository
    {
        void Add(Order order);
        IEnumerable<Order> GetAll();
    }

    // інтерфейс для контрактів сповіщень клієнтів
    public interface INotifier
    {
        void SendNotification(string clientName, string message);
    }

    // реалізація сервісу сповіщень у консоль
    public class ConsoleNotifier : INotifier
    {
        public void SendNotification(string clientName, string message)
        {
            Console.WriteLine($"[СМС-ПОВІДОМЛЕННЯ для клієнта {clientName}]: {message}");
        }
    }

    // власна колекція для зберігання замовлень, яка реалізує IEnumerable<Order> через yield return
    public class OrderRepository : IOrderRepository, IEnumerable<Order>
    {
        private List<Order> _orders = new List<Order>();

        public void Add(Order order)
        {
            _orders.Add(order);
        }

        public IEnumerable<Order> GetAll()
        {
            return _orders;
        }

        // реалізація ітератора за допомогою yield return для перебору через foreach
        public IEnumerator<Order> GetEnumerator()
        {
            foreach (var order in _orders)
            {
                // тут можна додати додаткову логіку під час перебору, якщо потрібно
                yield return order;
            }
        }

        // негенеридкова версія ітератора для сумісності з інтерфейсом IEnumerable
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
            Console.WriteLine("Запуск демонстрації інтерфейсів та власної ітерації (yield)...");
            Console.WriteLine();

            // створюємо екземпляр нашої власної колекції через інтерфейс (зменшення зв'язаності)
            IOrderRepository repository = new OrderRepository();
            INotifier notifier = new ConsoleNotifier();

            // наповнюємо нашу власну колекцію тестовими даними
            repository.Add(new Order { Id = 1, ClientName = "Олександр Коваленко", Category = "Кухні", TotalPrice = 45000m, AssemblyHours = 8, IsCompleted = true });
            repository.Add(new Order { Id = 2, ClientName = "Марина Шевченко", Category = "Шафи", TotalPrice = 18000m, AssemblyHours = 4, IsCompleted = true });
            repository.Add(new Order { Id = 3, ClientName = "Ігор Бондаренко", Category = "Офісні меблі", TotalPrice = 25000m, AssemblyHours = 5, IsCompleted = false });
            repository.Add(new Order { Id = 4, ClientName = "Світлана Мельник", Category = "Гардеробні", TotalPrice = 34000m, AssemblyHours = 6, IsCompleted = true });

            Console.WriteLine("1. Демонстрація перебору елементів ВЛАСНОЇ колекції через foreach (з використанням yield):");

            // перебираємо елементи за допомогою нашого власного ітератора
            foreach (var order in (OrderRepository)repository)
            {
                Console.WriteLine($"-> Замовлення #{order.Id} | Клієнт: {order.ClientName} | Категорія: {order.Category} | Сума: {order.TotalPrice} грн");
            }

            Console.WriteLine();
            Console.WriteLine("2. Демонстрація роботи через інтерфейси (контракти взаємодії):");

            // використовуємо залежність через інтерфейс сповіщень для виконаних замовлень
            foreach (var order in repository.GetAll())
            {
                if (order.IsCompleted)
                {
                    notifier.SendNotification(order.ClientName, "Ваші меблі успішно змонтовано. Дякуємо за замовлення!");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Роботу програми успішно завершено. Натисни будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}   