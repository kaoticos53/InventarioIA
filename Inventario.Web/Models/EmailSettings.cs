namespace Inventario.Web.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string SupportEmail { get; set; }
        public bool EnableSsl { get; set; }
        
        // Solo para compatibilidad con código existente
        public string ApiKey { get => SmtpPassword; set => SmtpPassword = value; }
        public string FromEmail { get => SenderEmail; set => SenderEmail = value; }
        public string FromName { get => SenderName; set => SenderName = value; }
        public string AppName { get; set; } = "GestMant - Gestión de Mantenimiento";
    }
}
