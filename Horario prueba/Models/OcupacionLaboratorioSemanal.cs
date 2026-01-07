using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    public class OcupacionLaboratorioSemanal
    {
        [Key]
        public int IdOcupacion { get; set; }

        public int IdLaboratorio { get; set; }

        // 0=Dom ... 6=Sab
        public int DiaSemana { get; set; }

        public int IdFranja { get; set; }

        // Navegaciones (opcionales)
        public Laboratorio? Laboratorio { get; set; }
        public FranjaHoraria? FranjaHoraria { get; set; }
    }
}
