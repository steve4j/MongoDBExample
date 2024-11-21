﻿using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace MongoDBIndexer
{
    public static class SQLHelper
    {
        const string connectionString = "Integrated Security=SSPI;Pooling=true;Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HackathonDb";

        public static int InsertData(SqlParameter[] parameters)
        {
            var columnNames = string.Join(", ", parameters.Select(p => p.ParameterName.TrimStart('@')));
            var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));

            var query = $"INSERT INTO [dbo].[File] ({columnNames}) VALUES ({parameterNames});";
            using var connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing INSERT: {ex.Message}", ex);
                throw;
            }
        }

        public static void SelectData()
        {
            string query = "SELECT * FROM [dbo].[File]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["ID"]}, Name: {reader["Name"]} Text: {reader["Text"]}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public static void SelectDataWithLikeFilter(string filter)
        {
            string query = "SELECT * FROM [dbo].[File]";

            if (!filter.IsNullOrEmpty())
            {
                query += $" where Text like '%{filter}%'";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["ID"]}, Name: {reader["Name"]} Text: {reader["Text"]}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
