
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.Api;

[ApiController]
[Route("api/prueba-seguridad")]
[Authorize(Policy = "Api")]
public class PruebaSeguridadController : ControllerBase
{
    [HttpGet("privado")]
    public IActionResult ObtenerRecursoProtegido()
    {
        return Ok(new
        {
            mensaje = "Acceso autorizado. El token JWT es válido.",
            usuario = User.Identity?.Name,
            roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList()
        });
    }

    [HttpGet("administrador")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Administrador")]
    public IActionResult RecursoAdministrador()
    {
        return Ok(new
        {
            mensaje = "Acceso autorizado para Administrador."
        });
    }

    [HttpGet("operativo")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Operativo")]
    public IActionResult RecursoOperativo()
    {
        return Ok(new
        {
            mensaje = "Acceso autorizado para Operativo."
        });
    }
}
