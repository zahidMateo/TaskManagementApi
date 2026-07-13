namespace UserManagementApi.Models
{
    /// <summary>
    /// Representa un usuario (desarrollador) en la base de datos de gestión de usuarios.
    /// </summary>
    public class User
    {
        /// <summary> Identificador único del usuario (ej. usr-a). </summary>
        public string Id { get; set; }

        /// <summary> Nombre completo del usuario. </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> Dirección de correo electrónico. </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary> Rol laboral o puesto asignado (ej. Desarrollador Senior). </summary>
        public string Role { get; set; } = string.Empty;
    }
}
