using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Data;

namespace ReportSystem.Controllers
{	
	[Route("diagnostics")]
    public class DbTestController : Controller
    {

        private readonly SqlConnectionFactory _connectionFactory;
        private readonly ILogger<DbTestController> _logger;

        public DbTestController(
            SqlConnectionFactory connectionFactory,
            ILogger<DbTestController> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

		[HttpGet("database")]
		public async Task<IActionResult> TestConnection()
		{
			try
			{
				await using var connection =
					_connectionFactory.CreateConnection();

				await connection.OpenAsync();

				var result = await connection.QuerySingleAsync<int>(
					"SELECT 1");

				return Content(
					$"Database connection successful. Query result: {result}");
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Database connection test failed.");

				return StatusCode(
					500,
					"Database connection failed. Check the application logs.");
			}
		}
    }
}
