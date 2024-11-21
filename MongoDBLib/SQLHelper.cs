using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace MongoDBLib
{
    public static class SQLHelper
    {
        private static string connectionString = "Integrated Security=SSPI;Pooling=true;Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HackathonDb";

        static SQLHelper()
        {
            if (Environment.MachineName == "IFINB33")
            {
                connectionString = "Encrypt=True;TrustServerCertificate=True;Integrated Security=SSPI;Pooling=true;Data Source=IFINB33;Initial Catalog=HackathonDb";
            }
        }

        public static string GetConnectionString()
        {
            return connectionString;
        }

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
            SelectDataWithLikeFilter(null);
        }

        public static void SelectDataWithLikeFilter(string filter, bool includeFullText= true)
        {
            string query = "SELECT * FROM [dbo].[File]";

            if (!filter.IsNullOrEmpty())
            {
                query += $" where Text like '%{filter}%'";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        stopwatch.Stop();
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["ID"]}, Name: {reader["Name"]}");
                            if (includeFullText) Console.WriteLine($"Text: {reader["Text"]}");
                        }
                        Console.WriteLine($"Query executed in: {stopwatch.ElapsedMilliseconds} ms");
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
