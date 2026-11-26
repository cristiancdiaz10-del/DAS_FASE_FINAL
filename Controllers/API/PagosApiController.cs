using DAS_Proyecto.DTOs.Pagos;
using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.API
{
    [ApiController]
    [Route("api/pagos")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class PagosApiController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosApiController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        /// <summary>Obtiene el listado completo de pagos registrados.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PagoResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var pagos = await _pagoService.ObtenerTodosAsync();
            var response = pagos.Select(MapToResponseDto);
            return Ok(response);
        }

        /// <summary>Obtiene un pago específico por su ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PagoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var pago = await _pagoService.ObtenerPorIdAsync(id);
            if (pago == null)
                return NotFound(new { mensaje = $"No se encontró el pago con ID {id}." });

            return Ok(MapToResponseDto(pago));
        }

        /// <summary>Obtiene todos los pagos asociados a una orden de trabajo y el total abonado.</summary>
        [HttpGet("orden/{idOrdenTrabajo:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOrden(int idOrdenTrabajo)
        {
            var pagos = await _pagoService.ObtenerPorOrdenAsync(idOrdenTrabajo);
            var totalPagado = await _pagoService.ObtenerTotalPagadoAsync(idOrdenTrabajo);

            return Ok(new
            {
                idOrdenTrabajo,
                totalPagado,
                pagos = pagos.Select(MapToResponseDto)
            });
        }

        /// <summary>Registra un nuevo pago (Anticipo, Abono o SaldoFinal) para una orden de trabajo.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(PagoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PagoCreateDto dto)
        {
            try
            {
                var resultado = await _pagoService.RegistrarPagoAsync(
                    dto.IdOrdenTrabajo,
                    dto.MontoPago,
                    dto.TipoPago,
                    dto.FechaPago);

                if (!resultado.Exito)
                    return BadRequest(new { mensaje = resultado.Mensaje });

                // Buscamos el pago recién creado por el código generado para devolver el objeto completo
                var pagosOrden = await _pagoService.ObtenerPorOrdenAsync(dto.IdOrdenTrabajo);
                var creado = pagosOrden.FirstOrDefault(p => p.CodigoPago == resultado.CodigoPago);

                if (creado != null)
                {
                    var response = MapToResponseDto(creado);
                    return CreatedAtAction(nameof(GetById), new { id = response.IdPago }, response);
                }

                return StatusCode(StatusCodes.Status201Created, new
                {
                    mensaje = resultado.Mensaje,
                    codigoPago = resultado.CodigoPago
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>Informa que los pagos registrados son inmutables por reglas de auditoría contable.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Update(int id, [FromBody] PagoUpdateDto dto)
        {
            if (id != dto.IdPago)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del pago." });

            return Conflict(new
            {
                mensaje = "Por reglas de integridad y auditoría contable, los pagos registrados no pueden editarse una vez emitidos."
            });
        }

        /// <summary>Informa que los pagos registrados no pueden eliminarse físicamente.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Delete(int id)
        {
            return Conflict(new
            {
                mensaje = "Por reglas de integridad y auditoría contable, los pagos registrados no pueden eliminarse."
            });
        }

        private static PagoResponseDto MapToResponseDto(Pago p) => new()
        {
            IdPago = p.IdPago,
            CodigoPago = p.CodigoPago,
            IdOrdenTrabajo = p.IdOrdenTrabajo,
            CodigoOrden = p.OrdenTrabajo?.CodigoOrden ?? string.Empty,
            ClienteNombre = p.OrdenTrabajo?.Cotizacion?.Cliente?.Nombre ?? "N/A",
            MontoPago = p.MontoPago,
            FechaPago = p.FechaPago,
            TipoPago = p.TipoPago.ToString()
        };
    }
}