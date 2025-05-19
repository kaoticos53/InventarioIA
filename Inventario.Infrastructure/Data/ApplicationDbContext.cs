using Inventario.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, RolPersonalizado, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<FichaAveria> FichasAveria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar nombres de tablas de Identity
            modelBuilder.Entity<RolPersonalizado>().ToTable("AspNetRoles");

            // Configuraciones de las entidades
            modelBuilder.Entity<Equipo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NumeroSerie).HasMaxLength(100);
                entity.Property(e => e.Modelo).HasMaxLength(100);
                entity.Property(e => e.Marca).HasMaxLength(100);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.Ubicacion)
                    .WithMany(u => u.Equipos)
                    .HasForeignKey(e => e.UbicacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioCreacion)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ubicacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Direccion).HasMaxLength(500);
            });

            modelBuilder.Entity<FichaAveria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SolucionAplicada);
                entity.Property(e => e.Comentarios);

                entity.HasOne(f => f.Equipo)
                    .WithMany(e => e.FichasAveria)
                    .HasForeignKey(f => f.EquipoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.UsuarioReporte)
                    .WithMany()
                    .HasForeignKey(f => f.UsuarioReporteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.UsuarioResolucion)
                    .WithMany()
                    .HasForeignKey(f => f.UsuarioResolucionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
