using DAS_Proyecto.Services.Implementations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DAS_Proyecto.Filters;

/// <summary>
/// Protege las operaciones API que usan el JWT almacenado en una cookie.
/// Los clientes externos que envían Authorization: Bearer no dependen de cookies
/// y no necesitan un token antifalsificación.
/// </summary>
public sealed class ApiAntiforgeryFilter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    public ApiAntiforgeryFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        var metodo = request.Method;

        if (HttpMethods.IsGet(metodo) ||
            HttpMethods.IsHead(metodo) ||
            HttpMethods.IsOptions(metodo) ||
            HttpMethods.IsTrace(metodo))
        {
            return;
        }

        var autorizacion = request.Headers["Authorization"].ToString();
        if (autorizacion.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return;

        if (!request.Cookies.ContainsKey(JwtTokenService.CookieName))
            return;

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(new
            {
                mensaje = "La solicitud no superó la validación antifalsificación. Actualiza la página e inténtalo nuevamente."
            });
        }
    }
}
