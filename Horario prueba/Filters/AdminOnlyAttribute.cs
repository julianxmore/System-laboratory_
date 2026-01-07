using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Horario_prueba.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var esAdmin = httpContext.Session.GetString("EsAdmin") == "true";

            // Si NO es admin, mándalo a sus reservas (no al menú)
            if (!esAdmin)
            {
                context.Result = new RedirectToActionResult("MisReservas", "Reservas", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
