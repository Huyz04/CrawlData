using CrawData.Model;
using Microsoft.EntityFrameworkCore;

namespace CrawData.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Paper> Papers { get; set; }
        public DbSet<Typee> Types { get; set; }
    }
}
