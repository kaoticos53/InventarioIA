using System.ComponentModel.DataAnnotations;

namespace Inventario.Core.DTOs
{
    public class EmailRequest
    {
        [Required(ErrorMessage = "El destinatario es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string To { get; set; } = string.Empty;

        [Required(ErrorMessage = "El asunto es requerido")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cuerpo del mensaje es requerido")]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;
    }

    public class EmailTemplateRequest : EmailRequest
    {
        [Required(ErrorMessage = "El nombre de la plantilla es requerido")]
        public string TemplateName { get; set; } = string.Empty;

        public Dictionary<string, string> TemplateData { get; set; } = new Dictionary<string, string>();
    }

    public class EmailResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
