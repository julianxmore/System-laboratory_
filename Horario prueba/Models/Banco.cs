using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    [Table("banco")]
    public class Banco
    {
        [Key]
        [Column("id_banco")]
        public int IdBanco { get; set; }

        [Required]
        [Column("id_laboratorio")]
        public int IdLaboratorio { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; }

        [Column("capacidad")]
        public int Capacidad { get; set; }

        [MaxLength(50)]
        [Column("estado")]
        public string Estado { get; set; }

        [MaxLength(500)]
        [Column("observaciones")]
        public string Observaciones { get; set; }

        [ForeignKey(nameof(IdLaboratorio))]
        [ValidateNever]
        public Laboratorio Laboratorio { get; set; }

        // 👇 Decimos que esta colección es el lado inverso de ReservaBanco.Banco
        [ValidateNever]
        [InverseProperty(nameof(ReservaBanco.Banco))]
        public ICollection<ReservaBanco> Reservas { get; set; } = new List<ReservaBanco>();
    }
}
