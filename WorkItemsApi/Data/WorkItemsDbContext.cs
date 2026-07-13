using Microsoft.EntityFrameworkCore;
using System;
using WorkItemsApi.Enums;
using WorkItemsApi.Models;

namespace WorkItemsApi.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core para la base de datos de tareas/ítems de trabajo (SQLite).
    /// </summary>
    public class WorkItemsDbContext : DbContext
    {
        /// <summary>
        /// Inicializa el contexto de la base de datos de tareas.
        /// </summary>
        public WorkItemsDbContext(DbContextOptions<WorkItemsDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Tabla de ítems de trabajo (tareas) en la base de datos.
        /// </summary>
        public DbSet<WorkItem> WorkItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Semillar los ítems de trabajo iniciales asignados al Usuario A ("usr-a") y al Usuario B ("usr-b")
            // para coincidir con la configuración del escenario del frontend
            modelBuilder.Entity<WorkItem>().HasData(
                new WorkItem
                {
                    Id = "tsk-1",
                    Title = "Tarea A1",
                    Description = "Trabajo relevante de A",
                    IsRelevant = true,
                    DueDate = new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Status = WorkItemStatus.Assigned,
                    AssignedUserId = "usr-a"
                },
                new WorkItem
                {
                    Id = "tsk-2",
                    Title = "Tarea A2",
                    Description = "Otro trabajo relevante de A",
                    IsRelevant = true,
                    DueDate = new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Status = WorkItemStatus.Assigned,
                    AssignedUserId = "usr-a"
                },
                new WorkItem
                {
                    Id = "tsk-3",
                    Title = "Tarea A3",
                    Description = "Trabajo no relevante de A",
                    IsRelevant = false,
                    DueDate = new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Status = WorkItemStatus.Assigned,
                    AssignedUserId = "usr-a"
                },
                new WorkItem
                {
                    Id = "tsk-4",
                    Title = "Tarea B1",
                    Description = "Trabajo no relevante de B",
                    IsRelevant = false,
                    DueDate = new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Status = WorkItemStatus.Assigned,
                    AssignedUserId = "usr-b"
                }
            );
        }
    }
}
