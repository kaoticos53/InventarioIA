using Inventario.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Data.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppSettings> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración única para AppSettings (solo un registro)
            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings
                {
                    Id = 1,
                    AppName = "GestMant",
                    Tagline = "Gestión de Mantenimiento",
                    Description = "Sistema de gestión de mantenimiento para empresas",
                    SupportEmail = "soporte@gestmant.com",
                    SupportPhone = "+1234567890",
                    CompanyAddress = "Calle Falsa 123, Ciudad, País",
                    CompanyName = "GestMant S.A.",
                    WebsiteUrl = "https://www.gestmant.com",
                    LogoUrl = "/images/logo.png",
                    PrimaryColor = "#4a6cf7",
                    SecondaryColor = "#6c5ce7",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "Sistema"
                });
        }
    }
}
