
using DAS_Proyecto.Models;
using DAS_Proyecto.Repositories.Interfaces;
using DAS_Proyecto.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DAS_Proyecto.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioRepository _repository;
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UsuariosController(
        IUsuarioRepository repository,
        UserManager<Usuario> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _repository = repository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var usuarios = await _repository.ObtenerTodosAsync(busqueda);
        ViewBag.Busqueda = busqueda;
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Password))
        {
            ModelState.AddModelError(
                nameof(modelo.Password),
                "La contraseña es obligatoria.");
        }

        if (!Enum.IsDefined(typeof(RolUsuario), modelo.Rol))
        {
            ModelState.AddModelError(
                nameof(modelo.Rol),
                "Selecciona un rol válido.");
        }

        if (!ModelState.IsValid)
            return View(modelo);

        var correo = modelo.Correo.Trim();

        var usuarioExistente =
            await _userManager.FindByEmailAsync(correo);

        if (usuarioExistente != null)
        {
            ModelState.AddModelError(
                nameof(modelo.Correo),
                "Ya existe un usuario con ese correo.");

            return View(modelo);
        }

        var nombreRol = modelo.Rol.ToString();

        var rolExiste = await _roleManager.RoleExistsAsync(nombreRol);

        if (!rolExiste)
        {
            ModelState.AddModelError(
                string.Empty,
                "El rol seleccionado no existe en Identity.");

            return View(modelo);
        }

        var usuario = new Usuario
        {
            Nombre = modelo.Nombre.Trim(),
            Correo = correo,
            Rol = modelo.Rol
        };

        var resultadoCrear = await _userManager.CreateAsync(
            usuario,
            modelo.Password!);

        if (!resultadoCrear.Succeeded)
        {
            AgregarErroresIdentity(resultadoCrear);
            return View(modelo);
        }

        var resultadoRol = await _userManager.AddToRoleAsync(
            usuario,
            nombreRol);

        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);
            AgregarErroresIdentity(resultadoRol);
            return View(modelo);
        }

        TempData["MensajeExito"] =
            "Usuario creado correctamente mediante ASP.NET Identity.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _userManager.FindByIdAsync(
            id.ToString());

        if (usuario == null)
            return NotFound();

        return View(new UsuarioViewModel
        {
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = usuario.Rol,
            Password = string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UsuarioViewModel modelo)
    {
        if (id != modelo.IdUsuario)
            return BadRequest();

        if (!Enum.IsDefined(typeof(RolUsuario), modelo.Rol))
        {
            ModelState.AddModelError(
                nameof(modelo.Rol),
                "Selecciona un rol válido.");
        }

        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _userManager.FindByIdAsync(
            id.ToString());

        if (usuario == null)
            return NotFound();

        var correo = modelo.Correo.Trim();

        var usuarioExistente =
            await _userManager.FindByEmailAsync(correo);

        if (usuarioExistente != null &&
            usuarioExistente.Id != usuario.Id)
        {
            ModelState.AddModelError(
                nameof(modelo.Correo),
                "Otro usuario ya utiliza ese correo.");

            return View(modelo);
        }

        var nombreRol = modelo.Rol.ToString();

        if (!await _roleManager.RoleExistsAsync(nombreRol))
        {
            ModelState.AddModelError(
                string.Empty,
                "El rol seleccionado no existe en Identity.");

            return View(modelo);
        }

        usuario.Nombre = modelo.Nombre.Trim();
        usuario.Correo = correo;
        usuario.Rol = modelo.Rol;

        var resultadoActualizar =
            await _userManager.UpdateAsync(usuario);

        if (!resultadoActualizar.Succeeded)
        {
            AgregarErroresIdentity(resultadoActualizar);
            return View(modelo);
        }

        if (!string.IsNullOrWhiteSpace(modelo.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(
                usuario);

            var resultadoPassword =
                await _userManager.ResetPasswordAsync(
                    usuario,
                    token,
                    modelo.Password);

            if (!resultadoPassword.Succeeded)
            {
                AgregarErroresIdentity(resultadoPassword);
                return View(modelo);
            }
        }

        var rolesActuales =
            await _userManager.GetRolesAsync(usuario);

        if (!rolesActuales.Contains(
            nombreRol,
            StringComparer.OrdinalIgnoreCase))
        {
            var resultadoAgregarRol =
                await _userManager.AddToRoleAsync(
                    usuario,
                    nombreRol);

            if (!resultadoAgregarRol.Succeeded)
            {
                AgregarErroresIdentity(resultadoAgregarRol);
                return View(modelo);
            }
        }

        var rolesAnteriores = rolesActuales
            .Where(r => !r.Equals(
                nombreRol,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (rolesAnteriores.Count > 0)
        {
            var resultadoEliminarRoles =
                await _userManager.RemoveFromRolesAsync(
                    usuario,
                    rolesAnteriores);

            if (!resultadoEliminarRoles.Succeeded)
            {
                AgregarErroresIdentity(resultadoEliminarRoles);
                return View(modelo);
            }
        }

        TempData["MensajeExito"] =
            "Usuario actualizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _userManager.FindByIdAsync(
            id.ToString());

        if (usuario == null)
        {
            TempData["MensajeError"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _userManager.DeleteAsync(usuario);

        if (resultado.Succeeded)
        {
            TempData["MensajeExito"] =
                "Usuario eliminado correctamente.";
        }
        else
        {
            TempData["MensajeError"] =
                "No se pudo eliminar el usuario. Comprueba si tiene cotizaciones asociadas.";

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private void AgregarErroresIdentity(IdentityResult resultado)
    {
        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }
}
