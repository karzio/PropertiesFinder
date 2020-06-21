using Microsoft.EntityFrameworkCore;
using Models;

namespace DatabaseConnection
{
    public class DatabaseContext: DbContext
    {
        readonly public static string ConnectionString = "Data Source=.;Initial Catalog=KarolZ149599;Integrated Security=True";

        public DbSet<Entry> Entries { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
