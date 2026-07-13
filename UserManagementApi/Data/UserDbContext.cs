using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Semillar los usuarios A y B para coincidir con el escenario del frontend
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = "usr-a",
                    Name = "Usuario A",
                    Email = "usuarioa@example.com",
                    Role = "Desarrollador Senior"
                },
                new User
                {
                    Id = "usr-b",
                    Name = "Usuario B",
                    Email = "usuariob@example.com",
                    Role = "Desarrollador Junior"
                }
            );
        }
    }
}
