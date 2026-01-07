using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Horario_prueba.Data;
using Horario_prueba.Filters;
using Horario_prueba.Models;
using System;
using System.Linq;

namespace Horario_prueba.Controllers
{
    [RequireLogin]
    [AdminOnly]
    public class LaboratoriosController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public LaboratoriosController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        // ✅ INDEX: lista de labs + sección de ocupaciones
        public IActionResult Index()
        {
            var labs = _context.Laboratorios.OrderBy(l => l.Nombre).ToList();

            // combos para la sección de ocupaciones
            ViewBag.Labs = labs.Select(l => new SelectListItem
            {
                Value = l.IdLaboratorio.ToString(),
                Text = $"{l.Nombre} (Sede: {l.Sede}, Piso: {l.Piso})"
            }).ToList();

            ViewBag.Dias = new[]
            {
                new SelectListItem("Lunes","1"),
                new SelectListItem("Martes","2"),
                new SelectListItem("Miércoles","3"),
                new SelectListItem("Jueves","4"),
                new SelectListItem("Viernes","5"),
                new SelectListItem("Sábado","6"),
            }.ToList();

            ViewBag.Franjas = _context.FranjasHorarias
                .OrderBy(f => f.HoraInicio)
                .Select(f => new SelectListItem
                {
                    Value = f.IdFranja.ToString(),
                    Text = f.Nombre
                }).ToList();

            // listado de ocupaciones ya guardadas
            var ocupaciones = _context.OcupacionesSemanales
                .Include(o => o.Laboratorio)
                .Include(o => o.FranjaHoraria)
                .OrderBy(o => o.IdLaboratorio)
                .ThenBy(o => o.DiaSemana)
                .ThenBy(o => o.FranjaHoraria.HoraInicio)
                .ToList();

            ViewBag.Ocupaciones = ocupaciones;

            return View(labs);
        }

        // GET: /Laboratorios/Crear
        public IActionResult Crear()
        {
            return View("Upsert", new Laboratorio());
        }

        // GET: /Laboratorios/Editar/5
        public IActionResult Editar(int id)
        {
            var lab = _context.Laboratorios.Find(id);
            if (lab == null) return NotFound();

            // ✅ ya NO metemos ocupaciones aquí
            return View("Upsert", lab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(Laboratorio lab)
        {
            if (!ModelState.IsValid)
                return View("Upsert", lab);

            if (lab.IdLaboratorio == 0)
                _context.Laboratorios.Add(lab);
            else
                _context.Laboratorios.Update(lab);

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Eliminar sí puede tener su propia vista
        public IActionResult Eliminar(int id)
        {
            var lab = _context.Laboratorios.Find(id);
            if (lab == null) return NotFound();
            return View(lab);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var lab = _context.Laboratorios.Find(id);
            if (lab == null) return NotFound();

            _context.Laboratorios.Remove(lab);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // ✅ NUEVO: guardar ocupación semanal (desde INDEX)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarOcupacionSemanal(int idLaboratorio, int diaSemana, int[] franjasSeleccionadas)
        {
            if (idLaboratorio == 0 || diaSemana == 0 || franjasSeleccionadas == null || franjasSeleccionadas.Length == 0)
            {
                TempData["MsgOcup"] = "Selecciona laboratorio, día y al menos una franja.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var idFranja in franjasSeleccionadas)
            {
                var existe = _context.OcupacionesSemanales.Any(o =>
                    o.IdLaboratorio == idLaboratorio &&
                    o.DiaSemana == diaSemana &&
                    o.IdFranja == idFranja
                );

                if (!existe)
                {
                    _context.OcupacionesSemanales.Add(new OcupacionLaboratorioSemanal
                    {
                        IdLaboratorio = idLaboratorio,
                        DiaSemana = diaSemana,
                        IdFranja = idFranja
                    });
                }
            }

            _context.SaveChanges();
            TempData["MsgOcup"] = "Horario ocupado guardado.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ quitar ocupación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarOcupacionSemanal(int idOcupacion)
        {
            var oc = _context.OcupacionesSemanales.Find(idOcupacion);
            if (oc != null)
            {
                _context.OcupacionesSemanales.Remove(oc);
                _context.SaveChanges();
            }

            TempData["MsgOcup"] = "Ocupación eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
