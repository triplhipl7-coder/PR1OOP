using System;
using System.Collections.Generic;
using System.Linq;

namespace FurnitureAnalytics
{
    // структура замовлення для нашої меблевої тематики
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Category { get; set; } // наприклад: Кухні, Шафи, Гардеробні
        public decimal TotalPrice { get; set; }
        public int AssemblyHours { get; set; }
        public bool IsCompleted { get; set; }
    }

    // клас для передачі розширених даних через подію
    public class OrderEventArgs : EventArgs
    {
        public Order Order { get; }
        public string ActionDescription { get; }
        public DateTime Timestamp { get; }

        public OrderEventArgs(Order order, string actionDescription)
        {
            Order = order;
            ActionDescription = actionDescription;
            Timestamp = DateTime.Now;
        }
    }

    // менеджер замовлень, який відповідає за бізнес-логіку та генерацію подій
    public class OrderManager
    {
        private List<Order> _orders = new List<Order>();

        // оголошуємо події предметної області за стандартом .NET
        public event EventHandler<OrderEventArgs> OrderCreated;
        public event EventHandler<OrderEventArgs> OrderStatusChanged;
        public event EventHandler<OrderEventArgs> OrderOperationPerformed;
        public event EventHandler<OrderEventArgs> BusinessRuleViolated;

        // операція створення сутності з генерацією відповідної події
        public void AddOrder(Order order)
        {
            if (order.TotalPrice <= 0)
            {
                // генеруємо подію порушення бізнес-правила
                BusinessRuleViolated?.Invoke(this, new OrderEventArgs(order, "Спроба додати замовлення з нульовою або від'ємною вартістю!"));
                return;
            }

            _orders.Add(order);
            OrderCreated?.Invoke(this, new OrderEventArgs(order, "Успішно створено нове замовлення"));
        }

        // операція зміни статусу замовлення
        public void CompleteOrder(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null && !order.IsCompleted)
            {
                order.IsCompleted = true;
                OrderStatusChanged?.Invoke(this, new OrderEventArgs(order, "Статус замовлення змінено на 'Виконано'"));
            }
        }

        // виконання операції над сутністю
        public void PerformAssembly(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                OrderOperationPerformed?.Invoke(this, new OrderEventArgs(order, $"Розпочато та виконано монтажні роботи тривалістю {order.AssemblyHours} годин"));
            }
        }
    }

    // ПІДПИСНИК 1: Сервіс сповіщень (формує повідомлення для користувача/клієнта)
    public class NotificationService
    {
        public void Register(OrderManager manager)
        {
            manager.OrderCreated += OnOrderCreated;
            manager.OrderStatusChanged += OnOrderStatusChanged;
            manager.BusinessRuleViolated += OnBusinessRuleViolated;
        }

        private void OnOrderCreated(object sender, EventArgs e)
        {
            var args = e as OrderEventArgs;
            Console.WriteLine($"[ПОВІДОМЛЕННЯ КЛІЄНТУ]: Шановний(-а) {args.Order.ClientName}, ваше замовлення на {args.Order.Category} прийняте в роботу!");
        }

        private void OnOrderStatusChanged(object sender, EventArgs e)
        {
            var args = e as OrderEventArgs;
            Console.WriteLine($"[ПОВІДОМЛЕННЯ КЛІЄНТУ]: Вітаємо! Ваші меблі ({args.Order.Category}) готові. Дякуємо, що обрали нас!");
        }

        private void OnBusinessRuleViolated(object sender, EventArgs e)
        {
            var args = e as OrderEventArgs;
            Console.WriteLine($"[ПОПЕРЕДЖЕННЯ СИСТЕМИ]: Помилка для клієнта {args.Order.ClientName} -> {args.ActionDescription}");
        }
    }

    // ПІДПИСНИК 2: Журнал подій та аудиту (виконує логування та статистичний підрахунок)
    public class AuditLogger
    {
        private int _totalEventsLogged = 0;
        private decimal _accumulatedRevenue = 0;

        public void Register(OrderManager manager)
        {
            manager.OrderCreated += OnEventOccurred;
            manager.OrderStatusChanged += OnEventOccurred;
            manager.OrderOperationPerformed += OnEventOccurred;
            manager.BusinessRuleViolated += OnEventOccurred;
        }

        private void OnEventOccurred(object sender, EventArgs e)
        {
            var args = e as OrderEventArgs;
            _totalEventsLogged++;

            if (args.Order.IsCompleted && args.ActionDescription.Contains("Виконано"))
            {
                _accumulatedRevenue += args.Order.TotalPrice;
            }

            Console.WriteLine($"[АУДИТ ЛОГ]: Час: {args.Timestamp:HH:mm:ss} | Подія: {args.ActionDescription} | Загалом подій у лозі: {_totalEventsLogged}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // налаштовуємо вивід української мови в консоль без спотворень
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Запуск демонстрації подієвої моделі та підсистеми сповіщень...");
            Console.WriteLine();

            // ініціалізуємо менеджер замовлень та незалежних підписників
            OrderManager manager = new OrderManager();
            NotificationService notifier = new NotificationService();
            AuditLogger auditLogger = new AuditLogger();

            // підписуємо сервіси на події менеджера (зниження зв'язаності компонентів)
            notifier.Register(manager);
            auditLogger.Register(manager);

            Console.WriteLine("1. Сценарій: Створення стандартних замовлень (генеруються події OrderCreated):");
            manager.AddOrder(new Order { Id = 1, ClientName = "Олександр Коваленко", Category = "Кухні", TotalPrice = 45000m, AssemblyHours = 8, IsCompleted = false });
            Console.WriteLine();
            manager.AddOrder(new Order { Id = 2, ClientName = "Марина Шевченко", Category = "Шафи", TotalPrice = 18000m, AssemblyHours = 4, IsCompleted = false });

            Console.WriteLine();
            Console.WriteLine("2. Сценарій: Порушення бізнес-правила (генерується подія BusinessRuleViolated):");
            manager.AddOrder(new Order { Id = 3, ClientName = "Тестовий Клієнт", Category = "Гардеробні", TotalPrice = 0m, AssemblyHours = 2, IsCompleted = false });

            Console.WriteLine();
            Console.WriteLine("3. Сценарій: Виконання операції монтажу (генерується подія OrderOperationPerformed):");
            manager.PerformAssembly(1);

            Console.WriteLine();
            Console.WriteLine("4. Сценарій: Зміна статусу замовлення (генерується подія OrderStatusChanged):");
            manager.CompleteOrder(1);

            Console.WriteLine();
            Console.WriteLine("Роботу програми успішно завершено. Натисни будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}