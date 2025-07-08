using System;
using Npgsql;


namespace BankingSystemV1
{
    class Program
    {


        static string[] usernames;
        static string[] passwords;
        static decimal[] balances;
        static int userCount = 0;
        static int maxUsers = 100;

        static void Main(string[] args)
        {
            string connectionString = "Host=localhost;Port=15502;Database=postgres;Username=postgres;Password=15502";
            usernames = new string[maxUsers];
            passwords = new string[maxUsers];
            balances = new decimal[maxUsers];
            userCount = 0;

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Банковская система v1.0");
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Вход");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите действие: ");

                int choice;

                if (int.TryParse(Console.ReadLine(), out choice))
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
            if (userCount >= maxUsers)
            {
                maxUsers = userCount;
            }

            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine();

            for (int i = 0; i < userCount; i++)
            {
                if (usernames[i] == username)
                {
                    Console.WriteLine("Пользователь с таким именем уже существует.");
                    return;
                }
            }

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            usernames[userCount] = username;
            passwords[userCount] = password;
            balances[userCount] = 0;
            userCount++;

            Console.WriteLine("Регистрация успешна!");
        }

        static void LoginUser()
        {
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine();

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            int userIndex = -1;
            for (int i = 0; i < userCount; i++)
            {
                if (usernames[i] == username && passwords[i] == password)
                {
                    userIndex = i;
                    break;
                }
            }

            if (userIndex == -1)
            {
                Console.WriteLine("Неверное имя пользователя или пароль.");
                return;
            }

            Console.WriteLine($"\nДобро пожаловать, {username}!");
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

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice))
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

        static void Transfer(int senderIndex)
        {
            Console.Write("Введите имя получателя: ");
            string recipientName = Console.ReadLine();

            int recipientIndex = -1;
            for (int i = 0; i < userCount; i++)
            {
                if (usernames[i] == recipientName && i != senderIndex)
                {
                    recipientIndex = i;
                    break;
                }
            }

            if (recipientIndex == -1)
            {
                Console.WriteLine("Получатель не найден или вы пытаетесь перевести себе.");
                return;
            }

            Console.Write("Введите сумму для перевода: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
            {
                if (amount <= balances[senderIndex])
                {
                    balances[senderIndex] -= amount;
                    balances[recipientIndex] += amount;
                    Console.WriteLine($"Перевод {amount:C} пользователю {usernames[recipientIndex]} выполнен успешно.");
                    Console.WriteLine($"Ваш новый баланс: {balances[senderIndex]:C}");
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