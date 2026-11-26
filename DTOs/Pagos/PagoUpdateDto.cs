using System.ComponentModel.DataAnnotations;
using DAS_Proyecto.Models;

namespace DAS_Proyecto.DTOs.Pagos
{
    public class PagoUpdateDto
    {
        [Required(ErrorMessage = "El ID del pago es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del pago debe ser válido.")]
        public int IdPago { get; set; }

        [Range(0.01, 999999999999.99, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal MontoPago { get; set; }

        [EnumDataType(typeof(TipoPago), ErrorMessage = "Tipo de pago no válido (Anticipo, Abono o SaldoFinal).")]
        public TipoPago TipoPago { get; set; }

        public DateTime FechaPago { get; set; }
    }
}
