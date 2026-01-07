using Microsoft.AspNetCore.Mvc;      // 👈 AGREGA ESTO
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    [Table("laboratorio")]
    public class Laboratorio
    {
        [Key]
        [Column("id_laboratorio")]
        public int IdLaboratorio { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [MaxLength(100)]
        [Column("sede")]
        public string Sede { get; set; }

        [MaxLength(20)]
        [Column("piso")]
        public string Piso { get; set; }

        [MaxLength(500)]
        [Column("observaciones")]
        public string Observaciones { get; set; }

        // 🔴 AQUÍ EL CAMBIO
                   // inicializada
    }
}
