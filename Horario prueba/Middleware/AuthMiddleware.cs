using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horario_prueba.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _publicRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/",
            "/home/login",
            "/home/settoken",
            "/home/logout",
            "/error"
        };

        private readonly HashSet<string> _protectedRoutes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/home/index",
            "/home/editarbasedatos",
            "/home/visualizacion",
            "/home/agendarhorario"
        };

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Verificar si es ruta pública
            if (IsPublicRoute(path) || IsStaticFile(path))
            {
                await _next(context);
                return;
            }

            // Verificación estricta para rutas protegidas
            if (IsProtectedRoute(path))
            {
                var token = context.Session.GetString("FirebaseToken");

                if (string.IsNullOrEmpty(token))
                {
                    await HandleUnauthorized(context);
                    return;
                }

                try
                {
                    await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                    await _next(context);
                }
                catch
                {
                    await HandleUnauthorized(context);
                }
                return;
            }

            // Para otras rutas no definidas
            await _next(context);
        }

        private bool IsPublicRoute(string path)
        {
            return _publicRoutes.Contains(path);
        }

        private bool IsProtectedRoute(string path)
        {
            return _protectedRoutes.Contains(path) ||
                   path.StartsWith("/home/", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsStaticFile(string path)
        {
            return path.StartsWith("/css/") ||
                   path.StartsWith("/js/") ||
                   path.StartsWith("/lib/") ||
                   path.StartsWith("/imagenes/");
        }

        private async Task HandleUnauthorized(HttpContext context)
        {
            context.Session.Clear();

            // Evita el bucle de redirección
            if (context.Request.Path.Value.Equals("/home/login", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[AuthMiddleware] Ya estamos en Login. No redirigiendo.");
                return;
            }

            Console.WriteLine("[AuthMiddleware] Redirigiendo a Login.");
            context.Response.Redirect("/Home/Login", false);
            await Task.CompletedTask;
        }

    }
}