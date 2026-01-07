using Horario_prueba.Data;
using Microsoft.AspNetCore.Mvc;

namespace Horario_prueba.Controllers 
{
    public class AdminController : Controller
    {
        private readonly GestionLaboratorioContext _context;

        public AdminController(GestionLaboratorioContext context)
        {
            _context = context;
        }

        // Panel principal de administración
        public IActionResult MenuAdmin()
        {
            var esAdmin = HttpContext.Session.GetString("EsAdmin") == "true";
            var nombre = HttpContext.Session.GetString("Nombre") ?? "Administrador";

            if (!esAdmin)
            {
                // Si no es admin, mándalo a sus reservas normales
                return RedirectToAction("MisReservas", "Reservas");
            }

            ViewBag.Nombre = nombre;
            ViewBag.EsAdmin = true;

            return View();
        }
    }
}
