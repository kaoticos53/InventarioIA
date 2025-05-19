using System;
using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.Models
{
    public class AppSettings
    {
        public int Id { get; set; } = 1; // Siempre será 1 ya que solo tendremos una configuración
        
        [Required(ErrorMessage = "El nombre de la aplicación es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string AppName { get; set; } = "GestMant";
        
        [StringLength(200, ErrorMessage = "El eslogan no puede exceder los 200 caracteres")]
        public string Tagline { get; set; } = "Gestión de Mantenimiento";
        
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Description { get; set; } = "Sistema de gestión de mantenimiento para empresas";
        
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string SupportEmail { get; set; } = "soporte@gestmant.com";
        
        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        public string SupportPhone { get; set; } = "+1234567890";
        
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string CompanyAddress { get; set; } = "Calle Falsa 123, Ciudad, País";
        
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede exceder los 100 caracteres")]
        public string CompanyName { get; set; } = "GestMant S.A.";
        
        [Url(ErrorMessage = "La URL del sitio web no es válida")]
        public string WebsiteUrl { get; set; } = "https://www.gestmant.com";
        
        [Url(ErrorMessage = "La URL del logo no es válida")]
        public string LogoUrl { get; set; } = "/images/logo.png";
        
        [StringLength(100, ErrorMessage = "El color primario no puede exceder los 100 caracteres")]
        public string PrimaryColor { get; set; } = "#4a6cf7";
        
        [StringLength(100, ErrorMessage = "El color secundario no puede exceder los 100 caracteres")]
        public string SecondaryColor { get; set; } = "#6c5ce7";
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = "Sistema";
    }
}
