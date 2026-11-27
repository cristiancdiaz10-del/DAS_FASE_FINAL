
using System.ComponentModel.DataAnnotations;
using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Implementations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IAntiforgery _antiforgery;

    public AuthController(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        JwtTokenService jwtTokenService,
        IAntiforgery antiforgery)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _antiforgery = antiforgery;
    }

    /// <summary>
    /// Valida las credenciales y emite un JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearToken(
        [FromBody] TokenRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(
            request.Correo.Trim());

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(
            usuario,
            request.Contrasena,
            lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = "Correo o contraseña incorrectos."
            });
        }

        var jwt = await _jwtTokenService.CrearTokenAsync(usuario);

        return Ok(new
        {
            access_token = jwt.Token,
            token_type = "Bearer",
            expires_at_utc = jwt.ExpiresAtUtc
        });
    }

    /// <summary>
    /// Renueva el JWT mientras la sesión Identity siga activa.
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> RenovarToken()
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "La sesión ya no es válida."
            });
        }

        var jwt = await _jwtTokenService.CrearTokenAsync(usuario);

        Response.Cookies.Append(
            JwtTokenService.CookieName,
            jwt.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Path = "/",
                Expires = new DateTimeOffset(jwt.ExpiresAtUtc)
            });

        return Ok(new
        {
            expires_at_utc = jwt.ExpiresAtUtc
        });
    }

    /// <summary>
    /// Genera un token antifalsificación para las solicitudes de la API.
    /// </summary>
    [HttpGet("antiforgery")]
    [Authorize(Policy = "Api")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObtenerTokenAntiforgery()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            requestToken = tokens.RequestToken
        });
    }
}

/// <summary>
/// Modelo para solicitar un JWT.
/// </summary>
public sealed class TokenRequest
{
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;
}
