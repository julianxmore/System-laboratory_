using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    [Table("franja_horaria")]
    public class FranjaHoraria
    {
        [Key]
        [Column("id_franja")]
        public int IdFranja { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required]
        [Column("hora_inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Column("hora_fin")]
        public TimeSpan HoraFin { get; set; }

        public ICollection<ReservaBanco> Reservas { get; set; }
    }
}
