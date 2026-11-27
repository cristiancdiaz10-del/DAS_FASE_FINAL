using System.ComponentModel.DataAnnotations;

namespace DAS_Proyecto.DTOs.Materiales
{
    public class MaterialUpdateDto
    {
        [Required(ErrorMessage = "El ID del material es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del material debe ser válido.")]
        public int IdMaterial { get; set; }

        [Required(ErrorMessage = "El código del material es obligatorio.")]
        public string CodigoMaterial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del material es obligatorio.")]
        public string NombreMaterial { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
        public decimal CantidadDisponible { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        public string UnidadMedida { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "El costo unitario no puede ser negativo.")]
        public decimal CostoUnitario { get; set; }

        public string? Descripcion { get; set; }
    }
}
