using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    [Table("reserva_banco")]
    public class ReservaBanco
    {
        [Key]
        [Column("id_reserva")]
        public int IdReserva { get; set; }

        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Column("id_banco")]
        public int IdBanco { get; set; }

        [Column("id_franja")]
        public int IdFranja { get; set; }

        // 👇 IMPORTANTE: esto es un DATE
        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column("estado")]
        public string Estado { get; set; }

        // 🔗 Relaciones: especificamos la FK correcta

        [ForeignKey(nameof(IdEstudiante))]
        public Estudiante Estudiante { get; set; }

        [ForeignKey(nameof(IdBanco))]
        public Banco Banco { get; set; }

        [ForeignKey(nameof(IdFranja))]
        public FranjaHoraria FranjaHoraria { get; set; }
    }


}
