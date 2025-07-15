using System;
using System.Collections.Generic;
using Npgsql;

namespace BankingSystemV1
{
    class Program
    {
        static List<string> usernames = new List<string>();
        static List<string> passwords = new List<string>();
        static List<decimal> balances = new List<decimal>();

        static void Main(string[] args)
        {
            string connectionString = "Host=localhost;Port=15502;Database=BANK;Username=postgres;Password=15502";

            using (NpgsqlConnection connect = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    Console.WriteLine("Подключение к базе данных успешно!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
                }
            }

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nБанковская система v1.0");
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Вход");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            RegisterUser();
                            break;
                        case 2:
                            LoginUser();
                            break;
                        case 3:
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Пожалуйста, введите число.");
                }
            }
        }

        static void RegisterUser()
        {
            Console.Write("Введите имя пользователя: ");
            string name = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Имя пользователя не может быть пустым.");
                return;
            }

            // Проверяем на существование пользователя (без учета регистра)
            if (usernames.Exists(u => u.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Пользователь с таким именем уже существует!");
                return;
            }

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Пароль не может быть пустым.");
                return;
            }

            usernames.Add(name);
            passwords.Add(password);
            balances.Add(0);

            Console.WriteLine("Регистрация успешна!");
        }

        static void LoginUser()
        {
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine().Trim();

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            int userIndex = -1;
            for (int i = 0; i < usernames.Count; i++)
            {
                if (usernames[i].Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    passwords[i] == password)
                {
                    userIndex = i;
                    break;
                }
            }

            if (userIndex == -1)
            {
                Console.WriteLine("Неверный логин или пароль");
                return;
            }

            Console.WriteLine($"\nДобро пожаловать, {usernames[userIndex]}!");
            UserMenu(userIndex);
        }

        static void UserMenu(int userIndex)
        {
            bool logout = false;
            while (!logout)
            {
                Console.WriteLine("\nМеню пользователя");
                Console.WriteLine($"Текущий баланс: {balances[userIndex]:C}");
                Console.WriteLine("1. Пополнить счет");
                Console.WriteLine("2. Снять средства");
                Console.WriteLine("3. Перевести другому пользователю");
                Console.WriteLine("4. Выйти из аккаунта");
                Console.Write("Выберите действие: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            Deposit(userIndex);
                            break;
                        case 2:
                            Withdraw(userIndex);
                            break;
                        case 3:
                            Transfer(userIndex);
                            break;
                        case 4:
                            logout = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Пожалуйста, введите число.");
                }
            }
        }

        static void Deposit(int userIndex)
        {
            Console.Write("Введите сумму для пополнения: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
            {
                balances[userIndex] += amount;
                Console.WriteLine($"Счет успешно пополнен на {amount:C}. Новый баланс: {balances[userIndex]:C}");
            }
            else
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
            }
        }

        static void Withdraw(int userIndex)
        {
            Console.Write("Введите сумму для снятия: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
            {
                if (amount <= balances[userIndex])
                {
                    balances[userIndex] -= amount;
                    Console.WriteLine($"Снято {amount:C}. Новый баланс: {balances[userIndex]:C}");
                }
                else
                {
                    Console.WriteLine("Недостаточно средств на счете.");
                }
            }
            else
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
            }
        }

        static void Transfer(int userIndex)
        {
            Console.Write("Введите имя получателя: ");
            string recipientName = Console.ReadLine().Trim();

            if (recipientName.Equals(usernames[userIndex], StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Нельзя переводить средства самому себе.");
                return;
            }

            int recipientIndex = usernames.FindIndex(u => u.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

            if (recipientIndex == -1)
            {
                Console.WriteLine("Получатель не найден.");
                return;
            }

            Console.Write("Введите сумму для перевода: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
            {
                if (amount <= balances[userIndex])
                {
                    balances[userIndex] -= amount;
                    balances[recipientIndex] += amount;
                    Console.WriteLine($"Перевод {amount:C} пользователю {usernames[recipientIndex]} выполнен успешно.");
                    Console.WriteLine($"Ваш новый баланс: {balances[userIndex]:C}");
                }
                else
                {
                    Console.WriteLine("Недостаточно средств на счете.");
                }
            }
            else
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
            }
        }
    }
}