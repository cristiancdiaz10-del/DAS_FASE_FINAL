using System.ComponentModel.DataAnnotations;

namespace DAS_Proyecto.DTOs.Ordenes
{
    public class OrdenTrabajoCreateDto
    {
        [Required(ErrorMessage = "El ID de la cotización es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cotización válida.")]
        public int IdCotizacion { get; set; }

        public DateTime? FechaEntregaEstimada { get; set; }
    }
}
