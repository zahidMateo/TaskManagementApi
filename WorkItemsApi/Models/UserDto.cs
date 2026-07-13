namespace WorkItemsApi.Models
{
    /// <summary>
    /// Objeto de transferencia de datos para representar un usuario obtenido desde UserManagementApi.
    /// </summary>
    public class UserDto
    {
        /// <summary> Identificador del usuario. </summary>
        public string Id { get; set; }

        /// <summary> Nombre completo del usuario. </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> Correo electrónico de contacto. </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary> Rol asignado en la organización. </summary>
        public string Role { get; set; } = string.Empty;
    }
}
