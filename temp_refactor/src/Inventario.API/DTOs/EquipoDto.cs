using System;
using System.Collections.Generic;

namespace Inventario.Core.DTOs
{
    public class EquipoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaFinGarantia { get; set; }
        public string Estado { get; set; } = "Disponible";
        public int UbicacionId { get; set; }
        public string? UbicacionNombre { get; set; }
        public string? UsuarioCreacionId { get; set; }
        public string? UsuarioCreacionNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public ICollection<FichaAveriaDto> FichasAveria { get; set; } = new List<FichaAveriaDto>();
    }

    public class CreateEquipoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFinGarantia { get; set; }
        public string Estado { get; set; } = "Disponible";
        public int UbicacionId { get; set; }
    }

    public class UpdateEquipoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaFinGarantia { get; set; }
        public string Estado { get; set; } = "Disponible";
        public int UbicacionId { get; set; }
    }
}
