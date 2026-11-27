using DAS_Proyecto.Models;
using DAS_Proyecto.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DAS_Proyecto.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public AccountController(
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Home/Login.cshtml");
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string usuario,
        string contrasena,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(contrasena))
        {
            ViewBag.MensajeError = "Ingresa tu correo y contraseña.";
            return View("~/Views/Home/Login.cshtml");
        }

        var correo = usuario.Trim();
        var cuenta = await _userManager.FindByEmailAsync(correo);

        if (cuenta == null)
        {
            ViewBag.MensajeError = "Correo o contraseña incorrectos.";
            return View("~/Views/Home/Login.cshtml");
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(
            cuenta,
            contrasena,
            lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            ViewBag.MensajeError = resultado.IsLockedOut
                ? "La cuenta está bloqueada temporalmente. Inténtalo más tarde."
                : "Correo o contraseña incorrectos.";
            return View("~/Views/Home/Login.cshtml");
        }

        await _signInManager.SignInWithClaimsAsync(
    cuenta,
    isPersistent: false,
    additionalClaims: new[]
    {
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.GivenName,
            cuenta.Nombre)
    });

        // JWT HttpOnly para consumir la API desde Fetch sin exponer el token a JavaScript.
        var jwt = await _jwtTokenService.CrearTokenAsync(cuenta);
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete(
            JwtTokenService.CookieName,
            new CookieOptions { Path = "/", Secure = Request.IsHttps, SameSite = SameSiteMode.Strict });
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return Content("No tienes permisos para acceder a esta sección.");
    }
}
