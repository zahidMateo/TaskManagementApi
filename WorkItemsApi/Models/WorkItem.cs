using System;
using WorkItemsApi.Enums;

namespace WorkItemsApi.Models
{
    /// <summary>
    /// Representa una tarea o ítem de trabajo en el microservicio WorkItemsApi.
    /// </summary>
    public class WorkItem
    {
        /// <summary> Identificador de la tarea (ej. tsk-1). </summary>
        public string Id { get; set; }

        /// <summary> Título o nombre corto de la tarea. </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary> Descripción detallada del trabajo. </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary> Indica si la tarea es de alta relevancia/criticidad. </summary>
        public bool IsRelevant { get; set; }

        /// <summary> Fecha de vencimiento o entrega límite. </summary>
        public DateTime DueDate { get; set; }

        /// <summary> Estado del ciclo de vida de la tarea. </summary>
        public WorkItemStatus Status { get; set; } = WorkItemStatus.Pending;

        /// <summary> Identificador del usuario asignado a esta tarea (null si está pendiente). </summary>
        public string AssignedUserId { get; set; }

        /// <summary> Clasificación de prioridad para la lista ordenada del usuario. </summary>
        public int SortOrder { get; set; } = 0;
    }
}
