using Microsoft.Data.SqlClient;

namespace MongoDBIndexer
{
    public static class SQLHelper
    {
        const string connectionString = "Integrated Security=SSPI;Pooling=true;Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HackathonDb";

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
    }
}
