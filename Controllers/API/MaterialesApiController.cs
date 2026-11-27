using DAS_Proyecto.DTOs.Materiales;
using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Interfaces;
using DAS_Proyecto.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.API
{
    [ApiController]
    [Route("api/materiales")]
    [Produces("application/json")]
    [Authorize(Policy = "Api")]
    public class MaterialesApiController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialesApiController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>Obtiene el listado de materiales con filtro opcional de búsqueda.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MaterialResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? busqueda, CancellationToken cancellationToken)
        {
            var materiales = await _materialService.ObtenerListadoAsync(busqueda, cancellationToken);
            var response = materiales.Select(MapToResponseDto);
            return Ok(response);
        }

        /// <summary>Obtiene un material específico por su ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MaterialResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var material = await _materialService.ObtenerPorIdAsync(id, cancellationToken);
            if (material == null)
                return NotFound(new { mensaje = $"No se encontró el material con ID {id}." });

            return Ok(MapToResponseDto(material));
        }

        /// <summary>Registra un nuevo material en el inventario.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(MaterialResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] MaterialCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var viewModel = new MaterialViewModel
                {
                    CodigoMaterial = dto.CodigoMaterial,
                    NombreMaterial = dto.NombreMaterial,
                    CantidadDisponible = dto.CantidadDisponible,
                    UnidadMedida = dto.UnidadMedida,
                    CostoUnitario = dto.CostoUnitario,
                    Descripcion = dto.Descripcion
                };

                await _materialService.CrearAsync(viewModel, cancellationToken);

                // Buscamos el material recién creado por su código o listado para obtener su IdMaterial generado
                var listado = await _materialService.ObtenerListadoAsync(dto.CodigoMaterial, cancellationToken);
                var creado = listado.FirstOrDefault(m => m.CodigoMaterial == dto.CodigoMaterial);

                if (creado != null)
                {
                    var response = MapToResponseDto(creado);
                    return CreatedAtAction(nameof(GetById), new { id = response.IdMaterial }, response);
                }

                return StatusCode(StatusCodes.Status201Created, new { mensaje = "Material creado correctamente." });
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

        /// <summary>Actualiza los datos de un material existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialUpdateDto dto, CancellationToken cancellationToken)
        {
            if (id != dto.IdMaterial)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del material." });

            try
            {
                var existente = await _materialService.ObtenerPorIdAsync(id, cancellationToken);
                if (existente == null)
                    return NotFound(new { mensaje = $"No se encontró el material con ID {id}." });

                var viewModel = new MaterialViewModel
                {
                    IdMaterial = dto.IdMaterial,
                    CodigoMaterial = dto.CodigoMaterial,
                    NombreMaterial = dto.NombreMaterial,
                    CantidadDisponible = dto.CantidadDisponible,
                    UnidadMedida = dto.UnidadMedida,
                    CostoUnitario = dto.CostoUnitario,
                    Descripcion = dto.Descripcion
                };

                await _materialService.ActualizarAsync(viewModel, cancellationToken);
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

        /// <summary>Elimina un material del inventario si no tiene dependencias.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var existente = await _materialService.ObtenerPorIdAsync(id, cancellationToken);
                if (existente == null)
                    return NotFound(new { mensaje = $"No se encontró el material con ID {id}." });

                await _materialService.EliminarAsync(id, cancellationToken);
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

        private static MaterialResponseDto MapToResponseDto(Material m) => new()
        {
            IdMaterial = m.IdMaterial,
            CodigoMaterial = m.CodigoMaterial,
            NombreMaterial = m.NombreMaterial,
            CantidadDisponible = m.CantidadDisponible,
            UnidadMedida = m.UnidadMedida,
            CostoUnitario = m.CostoUnitario,
            ValorInventario = m.CantidadDisponible * m.CostoUnitario,
            Descripcion = m.Descripcion
        };
    }
}
