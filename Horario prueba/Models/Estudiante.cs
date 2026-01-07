using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horario_prueba.Models
{
    [Table("estudiante")]
    public class Estudiante
    {
        [Key]
        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("codigo")]
        public string Codigo { get; set; }

        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [MaxLength(100)]
        [Column("programa")]
        public string Programa { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        [Column("email")] 
        public string Email { get; set; }

        [Column("es_admin")] 
        public bool EsAdmin { get; set; } = false;
        // ✅ NAVEGACIÓN: NUNCA required
        public List<ReservaBanco> Reservas { get; set; } = new(); 
    }
}
