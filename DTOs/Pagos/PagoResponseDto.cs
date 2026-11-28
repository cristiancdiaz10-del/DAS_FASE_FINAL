namespace DAS_Proyecto.DTOs.Pagos
{
    public class PagoResponseDto
    {
        public int IdPago { get; set; }
        public string CodigoPago { get; set; } = string.Empty;

        public int IdOrdenTrabajo { get; set; }
        public string CodigoOrden { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;

        public decimal MontoPago { get; set; }
        public DateTime FechaPago { get; set; }
        public string TipoPago { get; set; } = string.Empty;
    }
}