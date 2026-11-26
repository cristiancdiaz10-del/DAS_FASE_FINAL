namespace DAS_Proyecto.DTOs.Cotizaciones
{
    public class CotizacionResponseDto
    {
        public int IdCotizacion { get; set; }
        public string CodigoCotizacion { get; set; } = string.Empty;

        public int IdCliente { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;

        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;

        public int IdServicio { get; set; }
        public string ServicioNombre { get; set; } = string.Empty;

        public DateTime FechaCotizacion { get; set; }
        public string DescripcionTrabajo { get; set; } = string.Empty;

        public decimal CostoMateriales { get; set; }
        public decimal CostoManoObra { get; set; }
        public decimal TotalCotizacion { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}