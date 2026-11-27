using System.ComponentModel.DataAnnotations;
using DAS_Proyecto.Models;

namespace DAS_Proyecto.DTOs.Pagos
{
    public class PagoCreateDto
    {
        [Required(ErrorMessage = "El ID de la orden de trabajo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una orden de trabajo válida.")]
        public int IdOrdenTrabajo { get; set; }

        [Required(ErrorMessage = "El monto del pago es obligatorio.")]
        [Range(0.01, 999999999999.99, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal MontoPago { get; set; }

        [Required(ErrorMessage = "El tipo de pago es obligatorio.")]
        [EnumDataType(typeof(TipoPago), ErrorMessage = "Tipo de pago no válido (Anticipo, Abono o SaldoFinal).")]
        public TipoPago TipoPago { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;
    }
}