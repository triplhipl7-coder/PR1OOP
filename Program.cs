using System;
using System.Collections.Generic;
using System.Linq;

namespace FurnitureAnalytics
{
    // структура для представлення замовлення у нашій предметній області
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // наприклад: Кухні, Шафи, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public bool IsCompleted { get; set; }
    }

    // клас-аргумент для передачі даних через подію
    public class OrderEventArgs : EventArgs
    {
        public Order Order { get; }
        public string Message { get; }

        public OrderEventArgs(Order order, string message)
        {
            Order = order;
            Message = message;
        }
    }

    // менеджер замовлень, який керує колекцією та генерує події при зміні стану
    public class OrderManager
    {
        private List<Order> _orders = new List<Order>();

        // оголошуємо події системи за допомогою стандартного EventHandler
        public event EventHandler<OrderEventArgs> OrderCreated;
        public event EventHandler<OrderEventArgs> OrderStatusChanged;

        // метод додавання нового замовлення з генеруванням події створення
        public void AddOrder(Order order)
        {
            _orders.Add(order);

            // викликаємо подію, якщо на неї хтось підписався
            OrderCreated?.Invoke(this, new OrderEventArgs(order, "Створено нове замовлення в системі"));
        }

        // метод зміни статусу замовлення з генеруванням події зміни стану
        public void CompleteOrder(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null && !order.IsCompleted)
            {
                order.IsCompleted = true;

                // викликаємо подію зміни статусу
                OrderStatusChanged?.Invoke(this, new OrderEventArgs(order, "Статус замовлення змінено на 'Виконано'"));
            }
        }

        // СЦЕНАРІЙ 1: Фільтрація замовлень за допомогою делегата Predicate (делегування критерію пошуку)
        public List<Order> FindOrders(Predicate<Order> criteria)
        {
            List<Order> result = new List<Order>();
            foreach (var order in _orders)
            {
                if (criteria(order))
                {
                    result.Add(order);
                }
            }
            return result;
        }

        // СЦЕНАРІЙ 2: Обробка замовлень за допомогою делегата Action (делегування довільної дії над елементами)
        public void ProcessOrders(Action<Order> action)
        {
            foreach (var order in _orders)
            {
                action(order);
            }
        }
    }

    // ПІДПИСНИК 1: Логгер, який записує всі події системи у консоль
    public class OrderLogger
    {
        public void Subscribe(OrderManager manager)
        {
            manager.OrderCreated += OnOrderCreated;
            manager.OrderStatusChanged += OnOrderStatusChanged;
        }

        private void OnOrderCreated(object sender, OrderEventArgs e)
        {
            Console.WriteLine($"[ЛОГ СИСТЕМИ]: {e.Message} | Клієнт: {e.Order.ClientName} ({e.Order.Category})");
        }

        private void OnOrderStatusChanged(object sender, OrderEventArgs e)
        {
            Console.WriteLine($"[ЛОГ СИСТЕМИ]: {e.Message} | Клієнт: {e.Order.ClientName} | Сума: {e.Order.TotalPrice} грн");
        }
    }

    // ПІДПИСНИК 2: Трекер статистики, який реагує на події та оновлює показники в реальному часі
    public class StatisticsTracker
    {
        private int _createdCount = 0;
        private decimal _totalCompletedSum = 0;

        public void Subscribe(OrderManager manager)
        {
            manager.OrderCreated += OnOrderCreated;
            manager.OrderStatusChanged += OnOrderStatusChanged;
        }

        private void OnOrderCreated(object sender, OrderEventArgs e)
        {
            _createdCount++;
            Console.WriteLine($"[СТАТИСТИКА]: Загальна кількість створених замовлень оновлена -> {_createdCount}");
        }

        private void OnOrderStatusChanged(object sender, OrderEventArgs e)
        {
            if (e.Order.IsCompleted)
            {
                _totalCompletedSum += e.Order.TotalPrice;
                Console.WriteLine($"[СТАТИСТИКА]: Загальна виручка по виконаних замовленнях -> {_totalCompletedSum} грн");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // налаштовуємо вивід української мови в консоль без спотворень
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Запуск демонстрації делегатів, лямбда-виразів та подієвої моделі...");
            Console.WriteLine();

            // ініціалізуємо головні компоненти системи
            OrderManager manager = new OrderManager();
            OrderLogger logger = new OrderLogger();
            StatisticsTracker stats = new StatisticsTracker();

            // підписуємо компоненти на події менеджера (зниження зв'язаності)
            logger.Subscribe(manager);
            stats.Subscribe(manager);

            Console.WriteLine("1. Демонстрація генерації подій при створенні сутностей:");
            manager.AddOrder(new Order { Id = 1, ClientName = "Олександр Коваленко", Category = "Кухні", TotalPrice = 45000m, AssemblyHours = 8, IsCompleted = false });
            manager.AddOrder(new Order { Id = 2, ClientName = "Марина Шевченко", Category = "Шафи", TotalPrice = 18000m, AssemblyHours = 4, IsCompleted = false });

            Console.WriteLine();
            Console.WriteLine("2. Демонстрація генерації події при зміні статусу замовлення:");
            manager.CompleteOrder(1);

            Console.WriteLine();
            Console.WriteLine("3. Демонстрація делегування поведінки через Predicate (фільтрація за ціною понад 30000 грн):");
            var expensiveOrders = manager.FindOrders(o => o.TotalPrice > 30000m);
            foreach (var order in expensiveOrders)
            {
                Console.WriteLine($"-> Знайдено дорогий проєкт: {order.ClientName} ({order.Category}) на суму {order.TotalPrice} грн");
            }

            Console.WriteLine();
            Console.WriteLine("4. Демонстрація делегування поведінки через Action (кастомна обробка та вивід усіх замовлень):");
            manager.ProcessOrders(o =>
            {
                string status = o.IsCompleted ? "Виконано" : "В роботі";
                Console.WriteLine($"[Обробка Action] Замовлення #{o.Id} | {o.ClientName} | Статус: {status}");
            });

            Console.WriteLine();
            Console.WriteLine("Роботу програми успішно завершено. Натисни будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}