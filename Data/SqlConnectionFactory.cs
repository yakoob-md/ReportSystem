using Microsoft.Data.SqlClient;

namespace ReportSystem.Data
{
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            string? connString = configuration.GetConnectionString("DefaultConnection");

            if (connString != null) _connectionString = connString;
            else throw new Exception("Database connection string not found");
		}

        public SqlConnection CreateConnection() 
        {
            return new SqlConnection(_connectionString);
        }
    }
}
