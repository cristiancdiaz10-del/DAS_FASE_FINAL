using System.ComponentModel.DataAnnotations;

namespace DAS_Proyecto.DTOs.Materiales
{
    public class MaterialCreateDto
    {
        [Required(ErrorMessage = "El código del material es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
        public string CodigoMaterial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del material es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string NombreMaterial { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
        public decimal CantidadDisponible { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        [StringLength(40, ErrorMessage = "La unidad no puede superar los 40 caracteres.")]
        public string UnidadMedida { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "El costo unitario no puede ser negativo.")]
        public decimal CostoUnitario { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }
    }
}