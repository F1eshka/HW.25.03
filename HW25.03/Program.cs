using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace HW25_03
{
    public class DatabaseConfig
    {
        public string ProviderName { get; }
        public string ConnectionString { get; }

        public DatabaseConfig(string providerName, string connectionString)
        {
            ProviderName = providerName;
            ConnectionString = connectionString;
        }
    }

    public class DatabaseFactory
    {
        private readonly DbProviderFactory _factory;
        private readonly string _connectionString;

        public DatabaseFactory(DatabaseConfig config)
        {
            _factory = DbProviderFactories.GetFactory(config.ProviderName);
            _connectionString = config.ConnectionString;
        }

        public DbConnection CreateConnection()
        {
            DbConnection connection = _factory.CreateConnection();
            connection.ConnectionString = _connectionString;
            return connection;
        }

        public DbCommand CreateCommand(string commandText, DbConnection connection)
        {
            DbCommand command = _factory.CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        public DbParameter CreateParameter(string name, object value)
        {
            DbParameter parameter = _factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }

    public class ProductRepository
    {
        private readonly DatabaseFactory _factory;

        public ProductRepository(DatabaseFactory factory)
        {
            _factory = factory;
        }

        public void ConnectToDatabase()
        {
            try
            {
                using (DbConnection connection = _factory.CreateConnection())
                {
                    connection.Open();
                    Console.WriteLine("Подключение к базе данных успешно!");
                    Console.WriteLine($"Название базы данных: {connection.Database}");
                    Console.WriteLine($"Состояние: {connection.State}");
                }
            }
            catch (DbException ex)
            {
                Console.WriteLine("Ошибка подключения к базе данных:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Неизвестная ошибка:");
                Console.WriteLine(ex.Message);
            }
        }

        public void ShowAllProducts()
        {
            try
            {
                using (DbConnection connection = _factory.CreateConnection())
                {
                    connection.Open();
                    string query = "SELECT Name, Type, Color, Calories FROM Products";
                    using (DbCommand command = _factory.CreateCommand(query, connection))
                    {
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("Таблица пуста.");
                                return;
                            }

                            Console.WriteLine("\nСписок овощей и фруктов:");
                            Console.WriteLine($"{"Название",-20} {"Тип",-10} {"Цвет",-15} {"Калорийность",-12}");

                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["Name"],-20} {reader["Type"],-10} {reader["Color"],-15} {reader["Calories"],-12}");
                            }
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                Console.WriteLine("Ошибка при получении данных:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка:");
                Console.WriteLine(ex.Message);
            }
        }

        public void ShowMinCalories()
        {
            try
            {
                using (DbConnection connection = _factory.CreateConnection())
                {
                    connection.Open();
                    string query = "SELECT MIN(Calories) FROM Products";
                    using (DbCommand command = _factory.CreateCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result == DBNull.Value)
                        {
                            Console.WriteLine("Таблица пуста");
                        }
                        else
                        {
                            Console.WriteLine($"Минимальная калорийность: {result} кал");
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                Console.WriteLine("Ошибка при получении данных:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Неизвестная ошибка:");
                Console.WriteLine(ex.Message);
            }
        }

        public void ShowVegetablesCount()
        {
            try
            {
                using (DbConnection connection = _factory.CreateConnection())
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Products WHERE Type = @Type";
                    using (DbCommand command = _factory.CreateCommand(query, connection))
                    {
                        command.Parameters.Add(_factory.CreateParameter("@Type", "Овощ"));
                        int count = (int)command.ExecuteScalar();
                        Console.WriteLine($"Количество овощей: {count}");
                    }
                }
            }
            catch (DbException ex)
            {
                Console.WriteLine("Ошибка при получении данных:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Неизвестная ошибка:");
                Console.WriteLine(ex.Message);
            }
        }

        public void ShowProductsBelowCalories()
        {
            try
            {
                Console.Write("Введите максимальную калорийность: ");
                if (!int.TryParse(Console.ReadLine(), out int maxCalories) || maxCalories < 0)
                {
                    Console.WriteLine("Некорректное значение калорийности.");
                    return;
                }

                using (DbConnection connection = _factory.CreateConnection())
                {
                    connection.Open();
                    string query = "SELECT Name, Type, Color, Calories FROM Products WHERE Calories < @MaxCalories";
                    using (DbCommand command = _factory.CreateCommand(query, connection))
                    {
                        command.Parameters.Add(_factory.CreateParameter("@MaxCalories", maxCalories));
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine($"Нет овощей или фруктов с калорийностью ниже {maxCalories} ккал.");
                                return;
                            }

                            Console.WriteLine($"\nОвощи и фрукты с калорийностью ниже {maxCalories} ккал:");
                            Console.WriteLine($"{"Название",-20} {"Тип",-10} {"Цвет",-15} {"Калорийность",-12}");

                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["Name"],-20} {reader["Type"],-10} {reader["Color"],-15} {reader["Calories"],-12}");
                            }
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                Console.WriteLine("Ошибка:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Неизвестная ошибка:");
                Console.WriteLine(ex.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            var config = new DatabaseConfig("System.Data.SqlClient",
                @"Server=DESKTOP-Q4ID39U\SQLEXPRESS;Database=VAndF;Trusted_Connection=True;");
            var factory = new DatabaseFactory(config);
            var repository = new ProductRepository(factory);

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. -> Подключиться к базе данных");
                Console.WriteLine("2.  -> Показать все овощи и фрукты");
                Console.WriteLine("3.  -> Показать минимальную калорийность");
                Console.WriteLine("4.  -> Показать количество овощей");
                Console.WriteLine("5.  -> Показать овощи и фрукты с калорийностью ниже указанной");
                Console.WriteLine("6.  -> Выход");
                Console.Write("Выберите опцию (1-6): ");

                string choice = Console.ReadLine();
                if (!int.TryParse(choice, out int option) || option < 1 || option > 6)
                {
                    Console.WriteLine("Неверный выбор");
                    continue;
                }

                switch (option)
                {
                    case 1:
                        repository.ConnectToDatabase();
                        break;
                    case 2:
                        repository.ShowAllProducts();
                        break;
                    case 3:
                        repository.ShowMinCalories();
                        break;
                    case 4:
                        repository.ShowVegetablesCount();
                        break;
                    case 5:
                        repository.ShowProductsBelowCalories();
                        break;
                    case 6:
                        Console.WriteLine("Программа завершена");
                        return;
                }
            }
        }
    }
}