using DAS_Proyecto.DTOs.Ordenes;
using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Interfaces;
using DAS_Proyecto.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.API
{
    [ApiController]
    [Route("api/ordenes")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class OrdenesApiController : ControllerBase
    {
        private readonly IOrdenTrabajoService _ordenTrabajoService;

        public OrdenesApiController(IOrdenTrabajoService ordenTrabajoService)
        {
            _ordenTrabajoService = ordenTrabajoService;
        }

        /// <summary>Obtiene el listado de órdenes de trabajo con filtros opcionales de búsqueda y estado.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrdenTrabajoResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? busqueda,
            [FromQuery] EstadoOrden? estado,
            CancellationToken cancellationToken)
        {
            var ordenes = await _ordenTrabajoService.ObtenerListadoAsync(busqueda, estado, cancellationToken);
            var response = ordenes.Select(MapToResponseDto);
            return Ok(response);
        }

        /// <summary>Obtiene el detalle completo de una orden de trabajo, incluyendo pagos y materiales utilizados.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrdenTrabajoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var detalle = await _ordenTrabajoService.ObtenerDetalleAsync(id, cancellationToken);
            if (detalle == null || detalle.Orden == null)
                return NotFound(new { mensaje = $"No se encontró la orden de trabajo con ID {id}." });

            return Ok(MapDetalleToResponseDto(detalle));
        }

        /// <summary>Genera una nueva orden de trabajo a partir de una cotización aceptada.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrdenTrabajoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OrdenTrabajoCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var datosPrevios = await _ordenTrabajoService.ObtenerDatosParaCrearAsync(dto.IdCotizacion, cancellationToken);

                var modelo = new CrearOrdenViewModel
                {
                    IdCotizacion = dto.IdCotizacion,
                    CodigoCotizacion = datosPrevios.CodigoCotizacion,
                    Cliente = datosPrevios.Cliente,
                    Trabajo = datosPrevios.Trabajo,
                    Total = datosPrevios.Total,
                    FechaEntregaEstimada = dto.FechaEntregaEstimada
                };

                await _ordenTrabajoService.CrearDesdeCotizacionAsync(modelo, cancellationToken);

                // Buscamos la orden recién creada vinculada a esa cotización
                var listado = await _ordenTrabajoService.ObtenerListadoAsync(datosPrevios.CodigoCotizacion, null, cancellationToken);
                var creada = listado.FirstOrDefault(o => o.IdCotizacion == dto.IdCotizacion);

                if (creada != null)
                {
                    var response = MapToResponseDto(creada);
                    return CreatedAtAction(nameof(GetById), new { id = response.IdOrdenTrabajo }, response);
                }

                return StatusCode(StatusCodes.Status201Created, new { mensaje = "Orden de trabajo creada correctamente." });
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

        /// <summary>Actualiza el estado operativo de una orden de trabajo.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] OrdenTrabajoUpdateDto dto, CancellationToken cancellationToken)
        {
            if (id != dto.IdOrdenTrabajo)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la orden de trabajo." });

            try
            {
                await _ordenTrabajoService.ActualizarEstadoAsync(id, dto.Estado, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>Asigna un material del inventario a una orden de trabajo y descuenta existencias.</summary>
        [HttpPost("{id:int}/materiales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AgregarMaterial(int id, [FromBody] OrdenAgregarMaterialDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _ordenTrabajoService.AgregarMaterialAsync(id, dto.IdMaterial, dto.CantidadUtilizada, cancellationToken);
                return Ok(new { mensaje = "Material asignado correctamente a la orden de trabajo." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>Informa que las órdenes de trabajo no pueden eliminarse físicamente por reglas de trazabilidad.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Delete(int id)
        {
            return Conflict(new
            {
                mensaje = "Por reglas de integridad y trazabilidad del negocio, las órdenes de trabajo no se eliminan físicamente."
            });
        }

        private static OrdenTrabajoResponseDto MapToResponseDto(OrdenTrabajo o)
        {
            var totalCotizacion = o.Cotizacion?.TotalCotizacion ?? 0m;
            var totalPagado = o.Pagos?.Sum(p => p.MontoPago) ?? 0m;

            return new OrdenTrabajoResponseDto
            {
                IdOrdenTrabajo = o.IdOrdenTrabajo,
                CodigoOrden = o.CodigoOrden,
                IdCotizacion = o.IdCotizacion,
                CodigoCotizacion = o.Cotizacion?.CodigoCotizacion ?? string.Empty,
                ClienteNombre = o.Cotizacion?.Cliente?.Nombre ?? "N/A",
                DescripcionTrabajo = o.Cotizacion?.DescripcionTrabajo ?? string.Empty,
                FechaOrden = o.FechaOrden,
                FechaEntregaEstimada = o.FechaEntregaEstimada,
                FechaEntregaReal = o.FechaEntregaReal,
                TotalCotizacion = totalCotizacion,
                AnticipoRequerido = o.AnticipoRequerido,
                TotalPagado = totalPagado,
                SaldoPendiente = Math.Max(0m, totalCotizacion - totalPagado),
                Estado = o.Estado.ToString()
            };
        }

        private static OrdenTrabajoResponseDto MapDetalleToResponseDto(OrdenTrabajoDetalleViewModel vm)
        {
            var dto = MapToResponseDto(vm.Orden);
            dto.TotalPagado = vm.TotalPagado;
            dto.SaldoPendiente = vm.SaldoPendiente;
            dto.MaterialesUtilizados = vm.Detalles.Select(d => new OrdenMaterialDetalleDto
            {
                IdMaterial = d.IdMaterial,
                NombreMaterial = d.Material?.NombreMaterial ?? "N/A",
                UnidadMedida = d.Material?.UnidadMedida ?? string.Empty,
                CantidadUtilizada = d.CantidadUtilizada
            }).ToList();

            return dto;
        }
    }
}
