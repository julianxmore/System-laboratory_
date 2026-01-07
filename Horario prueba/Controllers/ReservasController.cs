using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Horario_prueba.Data;
using Horario_prueba.Filters;
using Horario_prueba.Models;
using Microsoft.AspNetCore.Http;

namespace Horario_prueba.Controllers
{
    [RequireLogin]
    public class ReservasController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public ReservasController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        // ----------------- Helpers -----------------

        private void CargarListas(CrearReservaViewModel vm)
        {
            vm.Laboratorios = _context.Laboratorios
                .OrderBy(l => l.Nombre)
                .ToList();

            vm.Bancos = _context.Bancos
                .Include(b => b.Laboratorio)
                .OrderBy(b => b.Laboratorio.Nombre)
                .ThenBy(b => b.Codigo)
                .ToList();

            vm.Franjas = _context.FranjasHorarias
                .OrderBy(f => f.HoraInicio)
                .ToList();
        }

        private bool EsFechaReservaValida(DateTime fecha)
        {
            var hoy = DateTime.Today;
            var f = fecha.Date;

            if (f < hoy) return false;

            if (f.DayOfWeek == DayOfWeek.Sunday)
                return false;

            if (hoy.DayOfWeek == DayOfWeek.Saturday)
            {
                var lunes = hoy.AddDays(2).Date;
                return f == hoy || f == lunes;
            }

            if (hoy.DayOfWeek == DayOfWeek.Sunday)
            {
                var lunes = hoy.AddDays(1).Date;
                return f == lunes;
            }

            var manana = hoy.AddDays(1).Date;
            return f == hoy || f == manana;
        }

        private bool EsFranjaValidaSegunHoraActual(DateTime fecha, int idFranja)
        {
            var hoy = DateTime.Today;

            if (fecha.Date != hoy)
                return true;

            var franja = _context.FranjasHorarias.Find(idFranja);
            if (franja == null) return false;

            var ahora = DateTime.Now.TimeOfDay;
            return franja.HoraInicio > ahora;
        }

        // ----------------- Crear reserva (estudiante) -----------------

        // GET: /Reservas/Crear
        public IActionResult Crear()
        {
            var vm = new CrearReservaViewModel
            {
                Fecha = DateTime.Today
            };

            CargarListas(vm);
            return View(vm);
        }

        // POST: /Reservas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearReservaViewModel vm)
        {
            CargarListas(vm);

            var fechaSeleccionada = vm.Fecha.Date;

            if (!EsFechaReservaValida(fechaSeleccionada))
            {
                ModelState.AddModelError("Fecha",
                    "Solo puedes reservar para hoy o mañana; si es sábado, hasta el lunes. No se permiten domingos ni fechas pasadas.");
            }

            if (vm.IdLaboratorio == 0)
                ModelState.AddModelError("IdLaboratorio", "Selecciona un laboratorio.");

            if (vm.IdBanco == 0)
                ModelState.AddModelError("IdBanco", "Selecciona un banco.");

            if (vm.IdFranja == 0)
                ModelState.AddModelError("IdFranja", "Selecciona una franja horaria.");

            if (vm.IdFranja != 0 && !EsFranjaValidaSegunHoraActual(fechaSeleccionada, vm.IdFranja))
            {
                ModelState.AddModelError("IdFranja",
                    "No puedes reservar una franja cuya hora de inicio ya pasó para el día de hoy.");
            }

            var idEstudiante = HttpContext.Session.GetInt32("IdEstudiante");
            if (idEstudiante == null)
                return RedirectToAction("Login", "Account");

            // ✅ Validar que el banco pertenezca al lab seleccionado
            var banco = _context.Bancos.Find(vm.IdBanco);
            if (banco == null || banco.IdLaboratorio != vm.IdLaboratorio)
            {
                ModelState.AddModelError(string.Empty, "El banco seleccionado no pertenece al laboratorio elegido.");
            }

            // ✅ NUEVO: bloqueo semanal por día de la semana
            if (banco != null && vm.IdFranja != 0)
            {
                int diaSemana = (int)fechaSeleccionada.DayOfWeek; // 0 dom ... 6 sab

                var bloqueado = _context.OcupacionesSemanales.Any(oc =>
                    oc.IdLaboratorio == banco.IdLaboratorio &&
                    oc.DiaSemana == diaSemana &&
                    oc.IdFranja == vm.IdFranja
                );

                if (bloqueado)
                {
                    ModelState.AddModelError(string.Empty,
                        "Ese laboratorio está ocupado por clase en esa franja (horario semanal).");
                }
            }

            // verificar que el banco esté libre en esa fecha/franja
            var ocupada = _context.ReservasBanco.Any(r =>
                r.IdBanco == vm.IdBanco &&
                r.IdFranja == vm.IdFranja &&
                r.Fecha == fechaSeleccionada &&
                r.Estado == "APROBADA"
            );

            if (ocupada)
            {
                ModelState.AddModelError(string.Empty,
                    "El banco ya está reservado en esa franja y fecha.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var reserva = new ReservaBanco
            {
                IdEstudiante = idEstudiante.Value,
                IdBanco = vm.IdBanco,
                IdFranja = vm.IdFranja,
                Fecha = fechaSeleccionada,
                Estado = "APROBADA"
            };

            _context.ReservasBanco.Add(reserva);
            _context.SaveChanges();

            return RedirectToAction(nameof(MisReservas));
        }

        // ----------------- Vista estudiante -----------------

        public IActionResult MisReservas()
        {
            var idEstudiante = HttpContext.Session.GetInt32("IdEstudiante");
            if (idEstudiante == null)
                return RedirectToAction("Login", "Account");

            var reservas = _context.ReservasBanco
                .Include(r => r.Banco)
                    .ThenInclude(b => b.Laboratorio)
                .Include(r => r.FranjaHoraria)
                .Where(r => r.IdEstudiante == idEstudiante.Value)
                .OrderByDescending(r => r.Fecha)
                .ThenBy(r => r.FranjaHoraria.HoraInicio)
                .ToList();

            return View(reservas);
        }

        // ----------------- Vista admin: listado -----------------

        public IActionResult AdminLista()
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            if (!esAdmin)
                return RedirectToAction(nameof(MisReservas));

            var hoy = DateTime.Today;

            if (hoy.DayOfWeek == DayOfWeek.Friday)
            {
                var antiguas = _context.ReservasBanco
                    .Where(r => r.Fecha < hoy)
                    .ToList();

                if (antiguas.Any())
                {
                    _context.ReservasBanco.RemoveRange(antiguas);
                    _context.SaveChanges();
                }
            }

            var reservasQuery = _context.ReservasBanco
                .Include(r => r.Banco)
                    .ThenInclude(b => b.Laboratorio)
                .Include(r => r.FranjaHoraria)
                .Include(r => r.Estudiante);

            var vigentes = reservasQuery
                .Where(r => r.Fecha >= hoy)
                .OrderBy(r => r.Fecha)
                .ThenBy(r => r.FranjaHoraria.HoraInicio)
                .ToList();

            var pasadas = reservasQuery
                .Where(r => r.Fecha < hoy)
                .OrderByDescending(r => r.Fecha)
                .ThenBy(r => r.FranjaHoraria.HoraInicio)
                .ToList();

            var vm = new AdminReservasViewModel
            {
                Vigentes = vigentes,
                Pasadas = pasadas
            };

            return View(vm);
        }

        // ----------------- Admin: Editar reserva -----------------

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            if (!esAdmin)
                return RedirectToAction(nameof(MisReservas));

            var reserva = _context.ReservasBanco
                .Include(r => r.Banco)
                .FirstOrDefault(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound();

            var vm = new CrearReservaViewModel
            {
                IdLaboratorio = reserva.Banco.IdLaboratorio,
                IdBanco = reserva.IdBanco,
                IdFranja = reserva.IdFranja,
                Fecha = reserva.Fecha.Date
            };

            CargarListas(vm);
            ViewBag.IdReserva = reserva.IdReserva;

            return View("AdminEditar", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, CrearReservaViewModel vm)
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            if (!esAdmin)
                return RedirectToAction(nameof(MisReservas));

            CargarListas(vm);
            var fechaSeleccionada = vm.Fecha.Date;

            if (!EsFechaReservaValida(fechaSeleccionada))
            {
                ModelState.AddModelError("Fecha",
                    "Solo puedes reservar para hoy o mañana; si es sábado, hasta el lunes. No se permiten domingos ni fechas pasadas.");
            }

            if (vm.IdLaboratorio == 0)
                ModelState.AddModelError("IdLaboratorio", "Selecciona un laboratorio.");

            if (vm.IdBanco == 0)
                ModelState.AddModelError("IdBanco", "Selecciona un banco.");

            if (vm.IdFranja == 0)
                ModelState.AddModelError("IdFranja", "Selecciona una franja horaria.");

            // validar banco pertenece a lab
            var banco = _context.Bancos.Find(vm.IdBanco);
            if (banco == null || banco.IdLaboratorio != vm.IdLaboratorio)
                ModelState.AddModelError(string.Empty, "El banco seleccionado no pertenece al laboratorio elegido.");

            // ✅ bloqueo semanal también para admin
            if (banco != null && vm.IdFranja != 0)
            {
                int diaSemana = (int)fechaSeleccionada.DayOfWeek;

                var bloqueado = _context.OcupacionesSemanales.Any(oc =>
                    oc.IdLaboratorio == banco.IdLaboratorio &&
                    oc.DiaSemana == diaSemana &&
                    oc.IdFranja == vm.IdFranja
                );

                if (bloqueado)
                    ModelState.AddModelError(string.Empty, "Ese laboratorio está ocupado por clase en esa franja (horario semanal).");
            }

            // ocupado por otra reserva
            var ocupada = _context.ReservasBanco.Any(r =>
                r.IdReserva != id &&
                r.IdBanco == vm.IdBanco &&
                r.IdFranja == vm.IdFranja &&
                r.Fecha == fechaSeleccionada &&
                r.Estado == "APROBADA"
            );

            if (ocupada)
                ModelState.AddModelError(string.Empty, "El banco ya está reservado en esa franja y fecha.");

            if (!ModelState.IsValid)
            {
                ViewBag.IdReserva = id;
                return View("AdminEditar", vm);
            }

            var reserva = _context.ReservasBanco.Find(id);
            if (reserva == null)
                return NotFound();

            reserva.IdBanco = vm.IdBanco;
            reserva.IdFranja = vm.IdFranja;
            reserva.Fecha = fechaSeleccionada;

            _context.SaveChanges();
            return RedirectToAction(nameof(AdminLista));
        }

        // ----------------- Admin: Eliminar reserva -----------------

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            if (!esAdmin)
                return RedirectToAction(nameof(MisReservas));

            var reserva = _context.ReservasBanco
                .Include(r => r.Banco)
                    .ThenInclude(b => b.Laboratorio)
                .Include(r => r.FranjaHoraria)
                .Include(r => r.Estudiante)
                .FirstOrDefault(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound();

            return View(reserva);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            if (!esAdmin)
                return RedirectToAction(nameof(MisReservas));

            var reserva = _context.ReservasBanco.Find(id);
            if (reserva == null)
                return NotFound();

            _context.ReservasBanco.Remove(reserva);
            _context.SaveChanges();

            return RedirectToAction(nameof(AdminLista));
        }

        // ----------------- AJAX -----------------

        [HttpGet]
        public IActionResult BancosDisponibles(int idLaboratorio, DateTime fecha)
        {
            var fechaSeleccionada = fecha.Date;
            var hoy = DateTime.Today;
            var ahora = DateTime.Now.TimeOfDay;
            var diaSemana = (int)fechaSeleccionada.DayOfWeek;

            var bancos = _context.Bancos
                .Include(b => b.Laboratorio)
                .Where(b => b.IdLaboratorio == idLaboratorio)
                .Where(b =>
                    _context.FranjasHorarias.Any(f =>
                        (fechaSeleccionada != hoy || f.HoraInicio > ahora)
                        && (fechaSeleccionada.DayOfWeek != DayOfWeek.Saturday || f.HoraFin <= new TimeSpan(14, 0, 0))

                        // ✅ NO ocupado por horario semanal
                        && !_context.OcupacionesSemanales.Any(oc =>
                            oc.IdLaboratorio == idLaboratorio &&
                            oc.DiaSemana == diaSemana &&
                            oc.IdFranja == f.IdFranja
                        )

                        && !_context.ReservasBanco.Any(r =>
                            r.IdBanco == b.IdBanco &&
                            r.IdFranja == f.IdFranja &&
                            r.Fecha == fechaSeleccionada &&
                            r.Estado == "APROBADA"
                        )
                    )
                )
                .Select(b => new
                {
                    idBanco = b.IdBanco,
                    texto = b.Laboratorio.Nombre + " - " + b.Codigo
                })
                .OrderBy(x => x.texto)
                .ToList();

            return Json(bancos);
        }

        [HttpGet]
        public IActionResult FranjasDisponibles(int idBanco, DateTime fecha)
        {
            var fechaSeleccionada = fecha.Date;
            var hoy = DateTime.Today;
            var ahora = DateTime.Now.TimeOfDay;

            var esSabado = fechaSeleccionada.DayOfWeek == DayOfWeek.Saturday;
            var limiteSabado = new TimeSpan(14, 0, 0);
            var diaSemana = (int)fechaSeleccionada.DayOfWeek;

            var idLab = _context.Bancos
                .Where(b => b.IdBanco == idBanco)
                .Select(b => b.IdLaboratorio)
                .FirstOrDefault();

            var franjas = _context.FranjasHorarias
                .Where(f =>
                    (!esSabado || f.HoraFin <= limiteSabado)

                    // ✅ NO ocupado por horario semanal
                    && !_context.OcupacionesSemanales.Any(oc =>
                        oc.IdLaboratorio == idLab &&
                        oc.DiaSemana == diaSemana &&
                        oc.IdFranja == f.IdFranja
                    )

                    && !_context.ReservasBanco.Any(r =>
                        r.IdBanco == idBanco &&
                        r.IdFranja == f.IdFranja &&
                        r.Fecha == fechaSeleccionada &&
                        r.Estado == "APROBADA"
                    )

                    && (fechaSeleccionada != hoy || f.HoraInicio > ahora)
                )
                .Select(f => new { f.IdFranja, f.Nombre })
                .ToList();

            return Json(franjas);
        } 
    }
}
