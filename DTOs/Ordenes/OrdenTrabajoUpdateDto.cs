using System.ComponentModel.DataAnnotations;
using DAS_Proyecto.Models;

namespace DAS_Proyecto.DTOs.Ordenes
{
    public class OrdenTrabajoUpdateDto
    {
        [Required(ErrorMessage = "El ID de la orden de trabajo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden debe ser válido.")]
        public int IdOrdenTrabajo { get; set; }

        [Required(ErrorMessage = "El nuevo estado de la orden es obligatorio.")]
        [EnumDataType(typeof(EstadoOrden), ErrorMessage = "Estado no válido (PendienteAnticipo, Recepcion, EnProceso, Listo o Entregado).")]
        public EstadoOrden Estado { get; set; }
    }
}
