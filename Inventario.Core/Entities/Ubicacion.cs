namespace Inventario.Core.Entities
{
    public class Ubicacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
        public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    }
}
