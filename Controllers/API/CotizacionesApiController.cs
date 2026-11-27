using DAS_Proyecto.DTOs.Cotizaciones;
using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.API
{
    [ApiController]
    [Route("api/cotizaciones")]
    [Produces("application/json")]
    [Authorize(Policy = "Api")]
    public class CotizacionesApiController : ControllerBase
    {
        private readonly ICotizacionService _cotizacionService;

        public CotizacionesApiController(ICotizacionService cotizacionService)
        {
            _cotizacionService = cotizacionService;
        }

        /// <summary>Obtiene el listado de cotizaciones con filtros opcionales de búsqueda y estado.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CotizacionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? busqueda,
            [FromQuery] EstadoCotizacion? estado,
            CancellationToken cancellationToken)
        {
            var cotizaciones = await _cotizacionService.ObtenerListadoAsync(busqueda, estado, cancellationToken);
            var response = cotizaciones.Select(MapToResponseDto);
            return Ok(response);
        }

        /// <summary>Obtiene una cotización específica por su ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CotizacionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var cotizacion = await _cotizacionService.ObtenerPorIdAsync(id, cancellationToken);
            if (cotizacion == null)
                return NotFound(new { mensaje = $"No se encontró la cotización con ID {id}." });

            return Ok(MapToResponseDto(cotizacion));
        }

        /// <summary>Crea una nueva cotización y calcula su total automáticamente.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CotizacionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CotizacionCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var nuevaCotizacion = new Cotizacion
                {
                    CodigoCotizacion = dto.CodigoCotizacion,
                    IdCliente = dto.IdCliente,
                    IdUsuario = dto.IdUsuario,
                    IdServicio = dto.IdServicio,
                    FechaCotizacion = dto.FechaCotizacion,
                    DescripcionTrabajo = dto.DescripcionTrabajo,
                    CostoMateriales = dto.CostoMateriales,
                    CostoManoObra = dto.CostoManoObra,
                    Estado = EstadoCotizacion.Pendiente
                };

                nuevaCotizacion.CalcularTotal();

                await _cotizacionService.CrearAsync(nuevaCotizacion, cancellationToken);

                // Recargamos la entidad creada para devolver también los nombres de Cliente/Usuario/Servicio si el servicio los incluye
                var creada = await _cotizacionService.ObtenerPorIdAsync(nuevaCotizacion.IdCotizacion, cancellationToken)
                             ?? nuevaCotizacion;

                var response = MapToResponseDto(creada);
                return CreatedAtAction(nameof(GetById), new { id = response.IdCotizacion }, response);
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

        /// <summary>Actualiza una cotización existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] CotizacionUpdateDto dto, CancellationToken cancellationToken)
        {
            if (id != dto.IdCotizacion)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la cotización." });

            try
            {
                var existente = await _cotizacionService.ObtenerPorIdAsync(id, cancellationToken);
                if (existente == null)
                    return NotFound(new { mensaje = $"No se encontró la cotización con ID {id}." });

                var datosActualizados = new Cotizacion
                {
                    IdCotizacion = dto.IdCotizacion,
                    CodigoCotizacion = dto.CodigoCotizacion,
                    IdCliente = dto.IdCliente,
                    IdUsuario = dto.IdUsuario,
                    IdServicio = dto.IdServicio,
                    FechaCotizacion = dto.FechaCotizacion,
                    DescripcionTrabajo = dto.DescripcionTrabajo,
                    CostoMateriales = dto.CostoMateriales,
                    CostoManoObra = dto.CostoManoObra,
                    Estado = dto.Estado
                };

                datosActualizados.CalcularTotal();

                await _cotizacionService.ActualizarAsync(id, datosActualizados, cancellationToken);
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

        /// <summary>Elimina una cotización existente si las reglas de negocio lo permiten.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existente = await _cotizacionService.ObtenerPorIdAsync(id, cancellationToken);
                if (existente == null)
                    return NotFound(new { mensaje = $"No se encontró la cotización con ID {id}." });

                await _cotizacionService.EliminarAsync(id, cancellationToken);
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
        }

        private static CotizacionResponseDto MapToResponseDto(Cotizacion c) => new()
        {
            IdCotizacion = c.IdCotizacion,
            CodigoCotizacion = c.CodigoCotizacion,
            IdCliente = c.IdCliente,
            ClienteNombre = c.Cliente?.Nombre ?? "N/A",
            IdUsuario = c.IdUsuario,
            UsuarioNombre = c.Usuario?.Nombre ?? "N/A",
            IdServicio = c.IdServicio,
            ServicioNombre = c.Servicio?.NombreServicio ?? "N/A",
            FechaCotizacion = c.FechaCotizacion,
            DescripcionTrabajo = c.DescripcionTrabajo,
            CostoMateriales = c.CostoMateriales,
            CostoManoObra = c.CostoManoObra,
            TotalCotizacion = c.TotalCotizacion,
            Estado = c.Estado.ToString()
        };
    }
}