
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DAS_Proyecto.Models
{
    public enum RolUsuario
    {
        Administrador,
        Operativo
    }

    public class Usuario : IdentityUser<int>
    {
        [NotMapped]
        public int IdUsuario
        {
            get => Id;
            set => Id = value;
        }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "El correo de acceso es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100)]
        public string Correo
        {
            get => Email ?? string.Empty;
            set
            {
                Email = value;
                UserName = value;
            }
        }

        public RolUsuario Rol { get; set; } = RolUsuario.Operativo;
    }
}
