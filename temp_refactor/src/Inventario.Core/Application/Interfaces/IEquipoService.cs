using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IEquipoService
    {
        Task<IEnumerable<EquipoDto>> GetAllEquiposAsync();
        Task<EquipoDto> GetEquipoByIdAsync(int id);
        Task<EquipoDto> CreateEquipoAsync(EquipoDto equipoDto, string userId);
        Task UpdateEquipoAsync(int id, EquipoDto equipoDto);
        Task DeleteEquipoAsync(int id);
    }
}
