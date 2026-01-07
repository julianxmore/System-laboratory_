using Horario_prueba.Data;
using Horario_prueba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Horario_prueba.Controllers
{
    public class AccountController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public AccountController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        // ---------- LOGIN ----------

        [HttpGet]
        public IActionResult Login()
        {
            // si ya está logueado, redirigimos según sea admin o estudiante
            var nombre = HttpContext.Session.GetString("Nombre");
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";

            if (!string.IsNullOrEmpty(nombre))
            {
                if (esAdmin)
                    return RedirectToAction("MenuAdmin", "Admin");
                else
                    return RedirectToAction("MisReservas", "Reservas");
            }

            return View();
        }

        public class LoginViewModel
        {
            [Required(ErrorMessage = "El código de estudiante es obligatorio")]
            [RegularExpression(@"^\d{11}$", ErrorMessage = "El código debe tener exactamente 11 dígitos numéricos")]
            public string Codigo { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "El correo institucional es obligatorio")]
            [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
            public string Email { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email?.Trim().ToLower();

            // 1) Intentar como ADMIN
            var admin = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.EsAdmin);

            if (admin != null)
            {
                var codigoIngresado = model.Codigo?.Trim();
                // ✅ email ya viene normalizado en "email"

                if (!string.Equals(admin.Codigo?.Trim(), codigoIngresado, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty,
                        "El código no coincide con el administrador registrado.");
                    return View(model);
                }

                HttpContext.Session.SetString("Nombre", admin.Nombre);
                HttpContext.Session.SetString("EsAdmin", "true");

                // (Opcional) si tu tabla usuarios tiene id, guarda también:
                HttpContext.Session.SetInt32("IdUsuario", admin.Id);

                return RedirectToAction("MenuAdmin", "Admin");
            }
             

            // 2) Intentar como ESTUDIANTE
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e =>
                    e.Codigo == model.Codigo || e.Email.ToLower() == email);

            if (estudiante != null)
            {
                if (!string.Equals(estudiante.Email, email, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(estudiante.Nombre, model.Nombre?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty,
                        "Los datos no coinciden con el estudiante registrado.");
                    return View(model);
                }
            }
            else
            {
                estudiante = new Estudiante
                {
                    Codigo = model.Codigo?.Trim(),
                    Nombre = model.Nombre?.Trim(),
                    Programa = "",
                    Email = email,
                    EsAdmin = false
                };

                _context.Estudiantes.Add(estudiante);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetString("Nombre", estudiante.Nombre);
            HttpContext.Session.SetString("EsAdmin", "false");
            HttpContext.Session.SetInt32("IdEstudiante", estudiante.IdEstudiante);

            return RedirectToAction("Crear", "Reservas");
        }

        // ---------- LOGOUT ----------

        // Acepta tanto GET como POST para que no dé 405
        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        } 
    }
}
