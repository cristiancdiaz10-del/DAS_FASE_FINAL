using System.ComponentModel.DataAnnotations;

namespace DAS_Proyecto.DTOs.Ordenes
{
    public class OrdenAgregarMaterialDto
    {
        [Required(ErrorMessage = "El ID del material es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un material válido.")]
        public int IdMaterial { get; set; }

        [Required(ErrorMessage = "La cantidad utilizada es obligatoria.")]
        [Range(0.01, 999999999, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public decimal CantidadUtilizada { get; set; }
    }
}