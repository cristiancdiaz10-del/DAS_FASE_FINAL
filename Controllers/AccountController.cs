
using DAS_Proyecto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DAS_Proyecto.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public AccountController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

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
                ViewBag.MensajeError =
                    "Ingresa tu correo y contraseña.";

                return View("~/Views/Home/Login.cshtml");
            }

            var correo = usuario.Trim();

            var cuenta = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == correo);

            if (cuenta == null)
            {
                ViewBag.MensajeError =
                    "Correo o contraseña incorrectos.";

                return View("~/Views/Home/Login.cshtml");
            }

            var resultado = await _signInManager.CheckPasswordSignInAsync(
                cuenta,
                contrasena,
                lockoutOnFailure: true);

            if (!resultado.Succeeded)
            {
                ViewBag.MensajeError =
                    "Correo o contraseña incorrectos.";

                return View("~/Views/Home/Login.cshtml");
            }

            await _signInManager.SignInAsync(
                cuenta,
                isPersistent: false);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content(
                "No tienes permisos para acceder a esta sección.");
        }
    }
}
