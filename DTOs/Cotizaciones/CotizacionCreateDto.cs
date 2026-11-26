using System.ComponentModel.DataAnnotations;

namespace DAS_Proyecto.DTOs.Cotizaciones
{
    public class CotizacionCreateDto
    {
        [Required(ErrorMessage = "El código de cotización es obligatorio.")]
        [StringLength(20, ErrorMessage = "El código no puede exceder los 20 caracteres.")]
        public string CodigoCotizacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un usuario válido.")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El servicio es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un servicio válido.")]
        public int IdServicio { get; set; }

        public DateTime FechaCotizacion { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La descripción del trabajo es obligatoria.")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
        public string DescripcionTrabajo { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "El costo de materiales no puede ser negativo.")]
        public decimal CostoMateriales { get; set; }

        [Range(0, 999999999, ErrorMessage = "El costo de mano de obra no puede ser negativo.")]
        public decimal CostoManoObra { get; set; }
    }
}