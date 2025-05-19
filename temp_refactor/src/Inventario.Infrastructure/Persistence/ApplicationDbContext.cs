using Inventario.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<FichaAveria> FichasAverias { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de tablas de Identity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Usuarios");
                entity.Property(u => u.Nombre).HasMaxLength(100);
                entity.Property(u => u.Apellido).HasMaxLength(100);
                entity.Property(u => u.ImagenUrl).HasMaxLength(500);
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UsuariosRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UsuariosClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UsuariosLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RolesClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UsuariosTokens");
            });

            // Configuración de la entidad Ubicacion
            builder.Entity<Ubicacion>(entity =>
            {
                entity.ToTable("Ubicaciones");
                
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(u => u.Descripcion).HasMaxLength(1000);
                entity.Property(u => u.Codigo).HasMaxLength(50);
                
                // Relación con Equipos
                entity.HasMany(u => u.Equipos)
                    .WithOne(e => e.Ubicacion)
                    .HasForeignKey(e => e.UbicacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la entidad Equipo
            builder.Entity<Equipo>(entity =>
            {
                entity.ToTable("Equipos");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NumeroSerie).HasMaxLength(100);
                entity.Property(e => e.Marca).HasMaxLength(100);
                entity.Property(e => e.Modelo).HasMaxLength(100);
                
                // Relación con Ubicacion
                entity.HasOne(e => e.Ubicacion)
                    .WithMany(u => u.Equipos)
                    .HasForeignKey(e => e.UbicacionId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con UsuarioCreacion
                entity.HasOne(e => e.UsuarioCreacion)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con FichasAveria
                entity.HasMany(e => e.FichasAveria)
                    .WithOne(f => f.Equipo)
                    .HasForeignKey(f => f.EquipoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de la entidad FichaAveria
            builder.Entity<FichaAveria>(entity =>
            {
                entity.ToTable("FichasAverias");
                
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(f => f.Descripcion).HasMaxLength(4000);
                entity.Property(f => f.Resolucion).HasMaxLength(4000);
                entity.Property(f => f.Estado).IsRequired().HasConversion<string>();
                entity.Property(f => f.Prioridad).IsRequired().HasConversion<string>();
                
                // Relación con Equipo
                entity.HasOne(f => f.Equipo)
                    .WithMany(e => e.FichasAveria)
                    .HasForeignKey(f => f.EquipoId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Relación con UsuarioReporte
                entity.HasOne(f => f.UsuarioReporte)
                    .WithMany()
                    .HasForeignKey(f => f.UsuarioReporteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con UsuarioResolucion
                entity.HasOne(f => f.UsuarioResolucion)
                    .WithMany()
                    .HasForeignKey(f => f.UsuarioResolucionId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con UsuarioAsignado
                entity.HasOne(f => f.UsuarioAsignado)
                    .WithMany()
                    .HasForeignKey(f => f.UsuarioAsignadoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de índices
            builder.Entity<Ubicacion>().HasIndex(u => u.Nombre).IsUnique();
            builder.Entity<Equipo>().HasIndex(e => e.NumeroSerie).IsUnique();
            builder.Entity<FichaAveria>().HasIndex(f => f.Estado);
            builder.Entity<FichaAveria>().HasIndex(f => f.Prioridad);
        }
    }
}
