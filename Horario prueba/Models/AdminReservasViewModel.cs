using System.Collections.Generic;

namespace Horario_prueba.Models
{
    public class AdminReservasViewModel
    {
        public List<ReservaBanco> Vigentes { get; set; } = new();
        public List<ReservaBanco> Pasadas { get; set; } = new();
    }
}
