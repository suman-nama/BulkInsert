using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

class Program
{
    static void Main()
    {
        string connectionString = "Host=localhost:5432;Database=Tejas;Username=postgres;Password=Tejas@2012";

        // Example data to be inserted
        List<User> users = GetUsers();

        BulkInsertUsers(connectionString, users);
    }

    static List<User> GetUsers()
    {
        // This method should return a list of users to be inserted.
        // For example, let's create some dummy data:
        List<User> users = new List<User>();
        for (int i = 1; i <= 1000; i++)
        {
            users.Add(new User { Id = i, Name = $"User{i}" });
        }
        return users;
    }

    static void BulkInsertUsers(string connectionString, List<User> users)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    using (var writer = conn.BeginBinaryImport("COPY users (id, name) FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var user in users)
                        {
                            writer.StartRow();
                            writer.Write(user.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                            writer.Write(user.Name, NpgsqlTypes.NpgsqlDbType.Text);
                        }
                        writer.Complete();
                    }

                    transaction.Commit();
                    Console.WriteLine("Bulk insert committed successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error during bulk insert: " + ex.Message);
                }
            }
        }
    }
}

class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
