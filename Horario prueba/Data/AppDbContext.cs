using Microsoft.EntityFrameworkCore;
using Horario_prueba.Models;

namespace Horario_prueba.Data
{
    public class GestionLaboratorioContext : DbContext
    {
        public GestionLaboratorioContext(DbContextOptions<GestionLaboratorioContext> options)
            : base(options) { }

        public DbSet<Laboratorio> Laboratorios { get; set; }
        public DbSet<Banco> Bancos { get; set; }
        public DbSet<FranjaHoraria> FranjasHorarias { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<ReservaBanco> ReservasBanco { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } = default!;

        // ✅ NUEVO
        public DbSet<OcupacionLaboratorioSemanal> OcupacionesSemanales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ mapeo explícito (evita nombres raros)
            modelBuilder.Entity<OcupacionLaboratorioSemanal>(entity =>
            {
                entity.ToTable("ocupaciones_semanales"); // <- nombre real en postgres (minúsculas)

                entity.HasKey(x => x.IdOcupacion);

                entity.Property(x => x.IdOcupacion).HasColumnName("id_ocupacion");
                entity.Property(x => x.IdLaboratorio).HasColumnName("id_laboratorio");
                entity.Property(x => x.DiaSemana).HasColumnName("dia_semana");
                entity.Property(x => x.IdFranja).HasColumnName("id_franja");

                entity.HasOne(x => x.Laboratorio)
                      .WithMany()
                      .HasForeignKey(x => x.IdLaboratorio);

                entity.HasOne(x => x.FranjaHoraria)
                      .WithMany()
                      .HasForeignKey(x => x.IdFranja);
            });
        }
    }
}
