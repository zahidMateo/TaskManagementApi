using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core para la base de datos de usuarios (SQLite).
    /// </summary>
    public class UserDbContext : DbContext
    {
        /// <summary>
        /// Inicializa el contexto de la base de datos de usuarios.
        /// </summary>
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Tabla de usuarios en la base de datos.
        /// </summary>
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
