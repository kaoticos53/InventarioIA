using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core
{
    public interface IEquipoService
    {
        Task<IEnumerable<EquipoDto>> GetAllEquiposAsync();
        Task<EquipoDto> GetEquipoByIdAsync(int id);
        Task<EquipoDto> CreateEquipoAsync(CreateEquipoDto equipoDto, string userId);
        Task UpdateEquipoAsync(int id, UpdateEquipoDto equipoDto);
        Task DeleteEquipoAsync(int id);
        Task<IEnumerable<EquipoDto>> GetEquiposByUbicacionAsync(int ubicacionId);
        Task<IEnumerable<EquipoDto>> GetEquiposByEstadoAsync(string estado);
    }
}
