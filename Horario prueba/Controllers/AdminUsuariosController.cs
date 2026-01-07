using System;
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
    public class AdminUsuariosController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public AdminUsuariosController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        // LISTA
        public IActionResult Index()
        {
            var usuarios = _context.Usuarios
                .OrderByDescending(u => u.EsAdmin)
                .ThenBy(u => u.Nombre)
                .ToList();

            return View(usuarios);
        }

        // CREAR (GET)
        [HttpGet]
        public IActionResult Crear()
        {
            return View("Upsert", new Usuario());
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Usuario u)
        {
            NormalizarUsuario(u);

            if (!ModelState.IsValid)
                return View("Upsert", u);

            // Duplicado por email
            if (_context.Usuarios.Any(x => x.Email.ToLower() == u.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
                return View("Upsert", u);
            }

            _context.Usuarios.Add(u);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // EDITAR (GET)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound();

            return View("Upsert", u);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Usuario u)
        {
            if (id != u.Id) return BadRequest();

            NormalizarUsuario(u);

            if (!ModelState.IsValid)
                return View("Upsert", u);

            // Duplicado por email (excepto el mismo)
            if (_context.Usuarios.Any(x => x.Id != u.Id && x.Email.ToLower() == u.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese correo.");
                return View("Upsert", u);
            }

            _context.Usuarios.Update(u);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR (GET)
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound();
            return View(u);
        }

        // ELIMINAR (POST)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var u = _context.Usuarios.Find(id);
            if (u == null) return NotFound();

            _context.Usuarios.Remove(u);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private void NormalizarUsuario(Usuario u)
        {
            u.Nombre = (u.Nombre ?? "").Trim();
            u.Codigo = (u.Codigo ?? "").Trim();
            u.Email = (u.Email ?? "").Trim().ToLower();
        }
    }
}
