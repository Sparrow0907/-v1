using System;
using Npgsql;

namespace BankingSystemV1
{
    class Program
    {
        static string connectionString = "Host=localhost;Port=15502;Database=BANK;Username=postgres;Password=15502";

        static void Main()
        {
            using (var connect = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connect.Open();
                    string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS users (
                        id SERIAL PRIMARY KEY,
                        username TEXT NOT NULL UNIQUE,
                        password TEXT NOT NULL,
                        balance NUMERIC NOT NULL DEFAULT 0
                    );";
                    using (var cmd = new NpgsqlCommand(createTableSql, connect))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка инициализации базы данных: {ex.Message}");
                }
            }
            ;

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nБанковская система v1.0");
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Вход");
                Console.WriteLine("3. Выход");
                Console.Write("Выберите действие: ");

                switch (GetUserChoice())
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
        }



        static int GetUserChoice()
        {
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                return choice;
            }
            Console.WriteLine("Пожалуйста, введите число.");
            return -1;
        }

        static void RegisterUser()
        {
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Имя пользователя не может быть пустым.");
                return;
            }

            using (var connect = new NpgsqlConnection(connectionString))
            {
                connect.Open();
                string checkSql = "SELECT COUNT(1) FROM users WHERE username = @username";
                using (var cmd = new NpgsqlCommand(checkSql, connect))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    if ((long)cmd.ExecuteScalar() > 0)
                    {
                        Console.WriteLine("Пользователь с таким именем уже существует!");
                        return;
                    }
                }
            }

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Пароль не может быть пустым.");
                return;
            }

            using (var connect = new NpgsqlConnection(connectionString))
            {
                connect.Open();
                string insertSql = "INSERT INTO users (username, password, balance) VALUES (@username, @password, 0)";
                using (var cmd = new NpgsqlCommand(insertSql, connect))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Регистрация успешна!");
        }

        static void LoginUser()
        {
            Console.Write("Введите имя пользователя: ");
            string username = Console.ReadLine().Trim();

            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            int userId = -1;
            using (var connect = new NpgsqlConnection(connectionString))
            {
                connect.Open();
                string checkSql = "SELECT id FROM users WHERE username = @username AND password = @password";
                using (var cmd = new NpgsqlCommand(checkSql, connect))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);
                    var result = cmd.ExecuteScalar();
                    userId = result != null ? (int)result : -1;
                }
            }

            switch (userId)
            {
                case -1:
                    Console.WriteLine("Неверный логин или пароль");
                    break;
                default:
                    Console.WriteLine($"\nДобро пожаловать, {username}!");
                    UserMenu(userId);
                    break;
            }
        }

        static void UserMenu(int userId)
        {
            bool logout = false;
            while (!logout)
            {
                decimal balance = GetUserBalance(userId);
                Console.WriteLine("\nМеню пользователя");
                Console.WriteLine($"Текущий баланс: {balance:C}");
                Console.WriteLine("1. Пополнить счет");
                Console.WriteLine("2. Снять средства");
                Console.WriteLine("3. Перевести другому пользователю");
                Console.WriteLine("4. Выйти из аккаунта");
                Console.Write("Выберите действие: ");

                switch (GetUserChoice())
                {
                    case 1:
                        Deposit(userId);
                        break;
                    case 2:
                        Withdraw(userId, balance);
                        break;
                    case 3:
                        Transfer(userId, balance);
                        break;
                    case 4:
                        logout = true;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }
            }
        }

        static decimal GetUserBalance(int userId)
        {
            using (var connect = new NpgsqlConnection(connectionString))
            {
                connect.Open();
                string sql = "SELECT balance FROM users WHERE id = @id";
                using (var cmd = new NpgsqlCommand(sql, connect))
                {
                    cmd.Parameters.AddWithValue("id", userId);
                    return (decimal)cmd.ExecuteScalar();
                }
            }
        }

        static void Deposit(int userId)
        {
            Console.Write("Введите сумму для пополнения: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
            {
                using (var connect = new NpgsqlConnection(connectionString))
                {
                    connect.Open();
                    string sql = "UPDATE users SET balance = balance + @amount WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(sql, connect))
                    {
                        cmd.Parameters.AddWithValue("amount", amount);
                        cmd.Parameters.AddWithValue("id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine($"Счет успешно пополнен на {amount:C}. Новый баланс: {GetUserBalance(userId):C}");
            }
            else
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
            }
        }

        static void Withdraw(int userId, decimal currentBalance)
        {
            Console.Write("Введите сумму для снятия: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                switch (amount)
                {
                    case <= 0:
                        Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
                        break;
                    case > 0 when amount <= currentBalance:
                        using (var connect = new NpgsqlConnection(connectionString))
                        {
                            connect.Open();
                            string sql = "UPDATE users SET balance = balance - @amount WHERE id = @id";
                            using (var cmd = new NpgsqlCommand(sql, connect))
                            {
                                cmd.Parameters.AddWithValue("amount", amount);
                                cmd.Parameters.AddWithValue("id", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine($"Снято {amount:C}. Новый баланс: {GetUserBalance(userId):C}");
                        break;
                    default:
                        Console.WriteLine("Недостаточно средств на счете.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите число.");
            }
        }

        static void Transfer(int senderId, decimal senderBalance)
        {
            Console.Write("Введите имя получателя: ");
            string recipientName = Console.ReadLine().Trim();

            int recipientId = -1;
            using (var connect = new NpgsqlConnection(connectionString))
            {
                connect.Open();
                string checkRecipientSql = "SELECT id FROM users WHERE username = @username AND id != @senderId";
                using (var cmd = new NpgsqlCommand(checkRecipientSql, connect))
                {
                    cmd.Parameters.AddWithValue("username", recipientName);
                    cmd.Parameters.AddWithValue("senderId", senderId);
                    var result = cmd.ExecuteScalar();
                    recipientId = result != null ? (int)result : -1;
                }
            }

            switch (recipientId)
            {
                case -1:
                    Console.WriteLine("Получатель не найден или вы пытаетесь перевести себе.");
                    return;
            }

            Console.Write("Введите сумму для перевода: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Неверная сумма. Пожалуйста, введите положительное число.");
                return;
            }

            switch (amount)
            {
                case > 0 when amount <= senderBalance:
                    using (var connect = new NpgsqlConnection(connectionString))
                    {
                        connect.Open();
                        using (var transaction = connect.BeginTransaction())
                        {
                            try
                            {
                                string updateSenderSql = "UPDATE users SET balance = balance - @amount WHERE id = @senderId";
                                string updateRecipientSql = "UPDATE users SET balance = balance + @amount WHERE id = @recipientId";

                                using (var cmd = new NpgsqlCommand(updateSenderSql, connect, transaction))
                                {
                                    cmd.Parameters.AddWithValue("amount", amount);
                                    cmd.Parameters.AddWithValue("senderId", senderId);
                                    cmd.ExecuteNonQuery();
                                }

                                using (var cmd = new NpgsqlCommand(updateRecipientSql, connect, transaction))
                                {
                                    cmd.Parameters.AddWithValue("amount", amount);
                                    cmd.Parameters.AddWithValue("recipientId", recipientId);
                                    cmd.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                Console.WriteLine($"Перевод {amount:C} пользователю {recipientName} выполнен успешно.");
                                Console.WriteLine($"Ваш новый баланс: {GetUserBalance(senderId):C}");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine($"Ошибка при переводе: {ex.Message}");
                            }
                        }
                    }
                    break;
                case > 0:
                    Console.WriteLine("Недостаточно средств на счете.");
                    break;
                default:
                    Console.WriteLine("Неверная сумма.");
                    break;
            }
        }
    }
}