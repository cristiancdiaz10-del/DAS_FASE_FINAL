namespace DAS_Proyecto.DTOs.Materiales
{
    public class MaterialResponseDto
    {
        public int IdMaterial { get; set; }
        public string CodigoMaterial { get; set; } = string.Empty;
        public string NombreMaterial { get; set; } = string.Empty;
        public decimal CantidadDisponible { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; }
        public decimal ValorInventario { get; set; }
        public string? Descripcion { get; set; }
    }
}