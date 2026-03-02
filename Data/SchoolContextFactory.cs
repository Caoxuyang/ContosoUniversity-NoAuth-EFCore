using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContosoUniversity.Data
{
    public static class SchoolContextFactory
    {
        private static string? _connectionString;

        public static void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static SchoolContext Create()
        {
            var connectionString = _connectionString
                ?? "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True";
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}

