using Microsoft.EntityFrameworkCore;

namespace SqlServerEFSample
{
    public class EFSampleContext : DbContext
    {
        public EFSampleContext(DbContextOptions options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
    }
}