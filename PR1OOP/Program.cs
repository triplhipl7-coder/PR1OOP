using System;
using System.Collections.Generic;

namespace OOP_Classes_Polymorphism
{
    // 1. Базовий клас сутності з унікальним ідентифікатором
    public abstract class Vehicle
    {
        // Інкапсульоване поле для ID (доступ лише для читання ззовні через властивість)
        private readonly int _id;
        public int Id => _id;

        // Інкапсульовані поля бренду та швидкості з валідацією
        private string _brand;
        private int _speed;

        public string Brand
        {
            get => _brand;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _brand = "Unknown";
                else
                    _brand = value;
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                // Валідація стану об'єкта: швидкість не може бути від'ємною
                if (value < 0)
                    _speed = 0;
                else
                    _speed = value;
            }
        }

        // Конструктор базового класу
        protected Vehicle(int id, string brand, int speed)
        {
            _id = id;
            Brand = brand;
            Speed = speed;
        }

        // Віртуальний метод для демонстрації поліморфізму
        public virtual void ShowInfo()
        {
            Console.WriteLine($"[Транспорт] ID: {Id}, Брендовий напис: {Brand}, Швидкість: {Speed} км/год");
        }
    }

    // 2. Перший похідний клас (Спортивний автомобіль)
    public class SportCar : Vehicle
    {
        private int _horsePower;

        public int HorsePower
        {
            get => _horsePower;
            set => _horsePower = value > 0 ? value : 100;
        }

        // Конструктор з використанням base()
        public SportCar(int id, string brand, int speed, int horsePower)
            : base(id, brand, speed)
        {
            HorsePower = horsePower;
        }

        // Перевизначення віртуального методу (поліморфізм)
        public override void ShowInfo()
        {
            Console.WriteLine($"[Спорткар] ID: {Id}, Марка: {Brand}, Швидкість: {Speed} км/год, Потужність: {HorsePower} к.с.");
        }
    }

    // 3. Другий похідний клас (Вантажівка)
    public class Truck : Vehicle
    {
        private double _loadCapacity; // вантажопідйомність у тоннах

        public double LoadCapacity
        {
            get => _loadCapacity;
            set => _loadCapacity = value > 0 ? value : 1.0;
        }

        public Truck(int id, string brand, int speed, double loadCapacity)
            : base(id, brand, speed)
        {
            LoadCapacity = loadCapacity;
        }

        // Перевизначення віртуального методу
        public override void ShowInfo()
        {
            Console.WriteLine($"[Вантажівка] ID: {Id}, Марка: {Brand}, Швидкість: {Speed} км/год, Вантажопідйомність: {LoadCapacity} т.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Створення колекції базового типу для демонстрації поліморфізму
            List<Vehicle> vehicles = new List<Vehicle>
            {
                new SportCar(1, "Ferrari", 320, 700),
                new Truck(2, "MAN", 120, 20.5),
                new SportCar(3, "Porsche", 290, 500)
            };

            Console.WriteLine("--- Демонстрація поліморфізму через колекцію базового типу ---");

            // Виклик віртуальних методів у циклі
            foreach (var vehicle in vehicles)
            {
                vehicle.ShowInfo(); // Викликається відповідний метод залежно від типу об'єкта
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}