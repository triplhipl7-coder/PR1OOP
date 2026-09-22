using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP_Classes_Polymorphism
{
    // Базовий клас сутності з унікальним ідентифікатором
    public abstract class Vehicle
    {
        private readonly int _id;
        public int Id => _id;

        private string _brand;
        private int _speed;

        public string Brand
        {
            get => _brand;
            set => _brand = string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
        }

        public int Speed
        {
            get => _speed;
            set => _speed = value < 0 ? 0 : value;
        }

        protected Vehicle(int id, string brand, int speed)
        {
            _id = id;
            Brand = brand;
            Speed = speed;
        }

        public virtual void ShowInfo()
        {
            Console.WriteLine($"[Транспорт] ID: {Id}, Марка: {Brand}, Швидкість: {Speed} км/год");
        }
    }

    public class SportCar : Vehicle
    {
        public int HorsePower { get; set; }

        public SportCar(int id, string brand, int speed, int horsePower) : base(id, brand, speed)
        {
            HorsePower = horsePower > 0 ? horsePower : 100;
        }

        public override void ShowInfo()
        {
            Console.WriteLine($"[Спорткар] ID: {Id}, Марка: {Brand}, Швидкість: {Speed} км/год, Потужність: {HorsePower} к.с.");
        }
    }

    public class Truck : Vehicle
    {
        public double LoadCapacity { get; set; }

        public Truck(int id, string brand, int speed, double loadCapacity) : base(id, brand, speed)
        {
            LoadCapacity = loadCapacity > 0 ? loadCapacity : 1.0;
        }

        public override void ShowInfo()
        {
            Console.WriteLine($"[Вантажівка] ID: {Id}, Марка: {Brand}, Швидкість: {Speed} км/год, Вантажопідйомність: {LoadCapacity} т.");
        }
    }

    // Додаткова сутність для зв'язування (Гараж, який керує парком машин)
    public class Garage
    {
        public string Name { get; set; }
        public int MaxCapacity { get; set; }

        public Garage(string name, int maxCapacity)
        {
            Name = name;
            MaxCapacity = maxCapacity;
        }
    }

    // Сервісний клас, який інкапсулює бізнес-логіку та управління сутностями
    public class VehicleService
    {
        private readonly List<Vehicle> _vehicles = new List<Vehicle>();
        private readonly Garage _garage = new Garage("Центральний гараж №1", 5); // Бізнес-обмеження місткості

        // Створення сутності з перевіркою бізнес-обмежень
        public bool AddVehicle(Vehicle vehicle)
        {
            if (_vehicles.Count >= _garage.MaxCapacity)
            {
                Console.WriteLine($"[Помилка бізнес-логіки] {_garage.Name} переповнений! Максимальна місткість: {_garage.MaxCapacity}");
                return false;
            }

            if (_vehicles.Any(v => v.Id == vehicle.Id))
            {
                Console.WriteLine($"[Помилка] Транспорт з ID {vehicle.Id} вже існує!");
                return false;
            }

            _vehicles.Add(vehicle);
            Console.WriteLine($"[Успіх] Транспорт успішно додано до {_garage.Name}!");
            return true;
        }

        // Пошук за ідентифікатором
        public Vehicle GetById(int id)
        {
            return _vehicles.FirstOrDefault(v => v.Id == id);
        }

        // Пошук за атрибутом (маркою)
        public List<Vehicle> GetByBrand(string brand)
        {
            return _vehicles.Where(v => v.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Оновлення стану сутності
        public bool UpdateVehicleSpeed(int id, int newSpeed)
        {
            var vehicle = GetById(id);
            if (vehicle == null)
            {
                Console.WriteLine("[Помилка] Транспорт не знайдено.");
                return false;
            }

            vehicle.Speed = newSpeed;
            Console.WriteLine($"[Успіх] Швидкість транспорту ID {id} оновлено до {newSpeed} км/год.");
            return true;
        }

        // Видалення сутності
        public bool RemoveVehicle(int id)
        {
            var vehicle = GetById(id);
            if (vehicle == null)
            {
                Console.WriteLine("[Помилка] Транспорт для видалення не знайдено.");
                return false;
            }

            _vehicles.Remove(vehicle);
            Console.WriteLine($"[Успіх] Транспорт з ID {id} видалено з системи.");
            return true;
        }

        // Виведення всіх елементів
        public void DisplayAll()
        {
            if (_vehicles.Count == 0)
            {
                Console.WriteLine("Гараж порожній.");
                return;
            }

            Console.WriteLine($"--- Список транспорту в {_garage.Name} ---");
            foreach (var vehicle in _vehicles)
            {
                vehicle.ShowInfo();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            VehicleService service = new VehicleService();

            // Початкове наповнення демонстраційними даними
            service.AddVehicle(new SportCar(1, "Ferrari", 320, 700));
            service.AddVehicle(new Truck(2, "MAN", 120, 20.5));

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n=== КОНСОЛЬНЕ МЕНЮ УПРАВЛІННЯ ТРАНСПОРТОМ ===");
                Console.WriteLine("1. Показати весь транспорт");
                Console.WriteLine("2. Додати новий спорткар");
                Console.WriteLine("3. Додати нову вантажівку");
                Console.WriteLine("4. Пошук транспорту за ID");
                Console.WriteLine("5. Оновити швидкість транспорту");
                Console.WriteLine("6. Видалити транспорт за ID");
                Console.WriteLine("7. Вихід");
                Console.Write("Виберіть пункт меню (1-7): ");

                string choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1":
                        service.DisplayAll();
                        break;
                    case "2":
                        try
                        {
                            Console.Write("Введіть ID: ");
                            int id = int.Parse(Console.ReadLine());
                            Console.Write("Введіть марку: ");
                            string brand = Console.ReadLine();
                            Console.Write("Введіть швидкість (км/год): ");
                            int speed = int.Parse(Console.ReadLine());
                            Console.Write("Введіть потужність (к.с.): ");
                            int hp = int.Parse(Console.ReadLine());

                            service.AddVehicle(new SportCar(id, brand, speed, hp));
                        }
                        catch
                        {
                            Console.WriteLine("Помилка введення даних!");
                        }
                        break;
                    case "3":
                        try
                        {
                            Console.Write("Введіть ID: ");
                            int id = int.Parse(Console.ReadLine());
                            Console.Write("Введіть марку: ");
                            string brand = Console.ReadLine();
                            Console.Write("Введіть швидкість (км/год): ");
                            int speed = int.Parse(Console.ReadLine());
                            Console.Write("Введіть вантажопідйомність (тонн): ");
                            double capacity = double.Parse(Console.ReadLine());

                            service.AddVehicle(new Truck(id, brand, speed, capacity));
                        }
                        catch
                        {
                            Console.WriteLine("Помилка введення даних!");
                        }
                        break;
                    case "4":
                        Console.Write("Введіть ID для пошуку: ");
                        if (int.TryParse(Console.ReadLine(), out int searchId))
                        {
                            var found = service.GetById(searchId);
                            if (found != null) found.ShowInfo();
                            else Console.WriteLine("Транспорт не знайдено.");
                        }
                        break;
                    case "5":
                        Console.Write("Введіть ID транспорту для оновлення швидкості: ");
                        if (int.TryParse(Console.ReadLine(), out int updateId))
                        {
                            Console.Write("Введіть нову швидкість: ");
                            if (int.TryParse(Console.ReadLine(), out int newSpeed))
                            {
                                service.UpdateVehicleSpeed(updateId, newSpeed);
                            }
                        }
                        break;
                    case "6":
                        Console.Write("Введіть ID для видалення: ");
                        if (int.TryParse(Console.ReadLine(), out int removeId))
                        {
                            service.RemoveVehicle(removeId);
                        }
                        break;
                    case "7":
                        exit = true;
                        Console.WriteLine("Роботу програми завершено.");
                        break;
                    default:
                        Console.WriteLine("Невірний вибір, спробуйте ще раз.");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }
    }
}
