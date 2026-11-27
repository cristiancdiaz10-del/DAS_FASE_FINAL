namespace DAS_Proyecto.DTOs.Ordenes
{
    public class OrdenTrabajoResponseDto
    {
        public int IdOrdenTrabajo { get; set; }
        public string CodigoOrden { get; set; } = string.Empty;

        public int IdCotizacion { get; set; }
        public string CodigoCotizacion { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string DescripcionTrabajo { get; set; } = string.Empty;

        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }
        public DateTime? FechaEntregaReal { get; set; }

        public decimal TotalCotizacion { get; set; }
        public decimal AnticipoRequerido { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string Estado { get; set; } = string.Empty;
        public List<OrdenMaterialDetalleDto> MaterialesUtilizados { get; set; } = new();
    }

    public class OrdenMaterialDetalleDto
    {
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CantidadUtilizada { get; set; }
    }
}
