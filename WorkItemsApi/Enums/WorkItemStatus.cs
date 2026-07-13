namespace WorkItemsApi.Enums
{
    /// <summary>
    /// Representa el estado de un ítem de trabajo (tarea) dentro del sistema.
    /// </summary>
    public enum WorkItemStatus
    {
        /// <summary> Tarea pendiente de asignación o inicio. </summary>
        Pending,
        /// <summary> Tarea asignada a un desarrollador. </summary>
        Assigned,
        /// <summary> Tarea completada exitosamente. </summary>
        Completed
    }
}
