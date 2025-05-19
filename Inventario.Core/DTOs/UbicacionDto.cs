using System.Collections.Generic;

namespace Inventario.Core.DTOs
{
    public class UbicacionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
        public int TotalEquipos { get; set; }
    }

    public class CreateUbicacionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class UpdateUbicacionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
