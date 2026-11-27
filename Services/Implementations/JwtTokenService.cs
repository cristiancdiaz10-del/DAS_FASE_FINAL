using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAS_Proyecto.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DAS_Proyecto.Services.Implementations;

/// <summary>
/// Centraliza la emisión de tokens JWT para el inicio de sesión web y la API.
/// </summary>
public sealed class JwtTokenService
{
    public const string CookieName = "Ranibal.ApiToken";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);

    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;

    public JwtTokenService(
        UserManager<Usuario> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<JwtTokenResult> CrearTokenAsync(Usuario usuario)
    {
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "La configuración JWT no está completa.");
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var ahora = DateTime.UtcNow;
        var expiracion = ahora.Add(TokenLifetime);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: ahora,
            expires: expiracion,
            signingCredentials: credentials);

        var valor = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(valor, expiracion);
    }
}

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
