using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Horario_prueba.Data;
using Horario_prueba.Filters;
using Horario_prueba.Models;

namespace Horario_prueba.Controllers
{
    [RequireLogin]
    [AdminOnly]
    public class BancosController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public BancosController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        private void CargarLaboratoriosEnViewBag()
        {
            var labs = _context.Laboratorios
                .OrderBy(l => l.Nombre)
                .ToList();

            ViewBag.Laboratorios = new SelectList(labs, "IdLaboratorio", "Nombre");
        }

        // GET: /Bancos
        public IActionResult Index()
        {
            var bancos = _context.Bancos
                .Include(b => b.Laboratorio)
                .OrderBy(b => b.Laboratorio.Nombre)
                .ThenBy(b => b.Codigo)
                .ToList();

            return View(bancos);
        }

        // GET: /Bancos/Crear  → muestra el formulario
        public IActionResult Crear()
        {
            CargarLaboratoriosEnViewBag();

            var banco = new Banco
            {
                Estado = "DISPONIBLE"
            };

            return View("Upsert", banco);   // usa la vista Upsert.cshtml
        }

        // GET: /Bancos/Editar/5  → carga el banco en el formulario
        public IActionResult Editar(int id)
        {
            var banco = _context.Bancos.Find(id);
            if (banco == null) return NotFound();

            CargarLaboratoriosEnViewBag();
            return View("Upsert", banco);
        }

        // POST: /Bancos/Guardar  → recibe el formulario (crear o editar)
        [HttpPost]
        public IActionResult Guardar(Banco banco)
        {
            if (!ModelState.IsValid)
            {
                CargarLaboratoriosEnViewBag();
                return View("Upsert", banco);
            }

            if (banco.IdBanco == 0)
                _context.Bancos.Add(banco);    // crear
            else
                _context.Bancos.Update(banco); // editar

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Bancos/Eliminar/5
        public IActionResult Eliminar(int id)
        {
            var banco = _context.Bancos
                .Include(b => b.Laboratorio)
                .FirstOrDefault(b => b.IdBanco == id);

            if (banco == null) return NotFound();
            return View(banco);
        }

        // POST: /Bancos/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        public IActionResult EliminarConfirmado(int id)
        {
            var banco = _context.Bancos.Find(id);
            if (banco == null) return NotFound();

            _context.Bancos.Remove(banco);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
