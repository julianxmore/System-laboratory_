using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Horario_prueba.Data;
using Horario_prueba.Filters;
using Horario_prueba.Models;

namespace Horario_prueba.Controllers
{
    [RequireLogin]
    [AdminOnly]
    public class AdminEstudiantesController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public AdminEstudiantesController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var estudiantes = _context.Estudiantes
                .OrderBy(e => e.Nombre)
                .ToList();

            return View(estudiantes);
        }

        // CREAR (GET)
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.EsAdmin = false;
            return View("Upsert", new Estudiante());
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Estudiante e, bool esAdmin)
        {
            NormalizarEstudiante(e);

            if (!ModelState.IsValid)
            {
                ViewBag.EsAdmin = esAdmin;
                return View("Upsert", e);
            }

            _context.Estudiantes.Add(e);
            _context.SaveChanges();

            SincronizarAdminPorEmail(e, esAdmin);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // EDITAR (GET)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var e = _context.Estudiantes.Find(id);
            if (e == null) return NotFound();

            var esAdmin = _context.Usuarios.Any(u => u.Email.ToLower() == (e.Email ?? "").ToLower() && u.EsAdmin);
            ViewBag.EsAdmin = esAdmin;

            return View("Upsert", e);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Estudiante e, bool esAdmin)
        {
            if (id != e.IdEstudiante) return BadRequest();

            NormalizarEstudiante(e);

            if (!ModelState.IsValid)
            {
                ViewBag.EsAdmin = esAdmin;
                return View("Upsert", e);
            }

            _context.Estudiantes.Update(e);
            _context.SaveChanges();

            SincronizarAdminPorEmail(e, esAdmin);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR (GET)
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var e = _context.Estudiantes.Find(id);
            if (e == null) return NotFound();
            return View(e);
        }

        // ELIMINAR (POST)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var e = _context.Estudiantes.Find(id);
            if (e == null) return NotFound();

            _context.Estudiantes.Remove(e);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private void NormalizarEstudiante(Estudiante e)
        {
            e.Nombre = (e.Nombre ?? "").Trim();
            e.Codigo = (e.Codigo ?? "").Trim();
            e.Email = (e.Email ?? "").Trim().ToLower();
        }

        private void SincronizarAdminPorEmail(Estudiante e, bool esAdmin)
        {
            var email = (e.Email ?? "").ToLower();
            if (string.IsNullOrWhiteSpace(email)) return;

            var u = _context.Usuarios.FirstOrDefault(x => x.Email.ToLower() == email);

            if (esAdmin)
            {
                if (u == null)
                {
                    _context.Usuarios.Add(new Usuario
                    {
                        Nombre = e.Nombre,
                        Codigo = e.Codigo,
                        Email = email,
                        EsAdmin = true
                    });
                }
                else
                {
                    u.Nombre = e.Nombre;
                    u.Codigo = e.Codigo;
                    u.EsAdmin = true;
                    _context.Usuarios.Update(u);
                }
            }
            else
            {
                if (u != null)
                {
                    u.EsAdmin = false;
                    _context.Usuarios.Update(u);
                }
            }
        }
    }
}
