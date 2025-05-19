using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IUbicacionService
    {
        Task<IEnumerable<UbicacionDto>> GetAllUbicacionesAsync();
        Task<UbicacionDto> GetUbicacionByIdAsync(int id);
        Task<UbicacionDto> CreateUbicacionAsync(UbicacionDto ubicacionDto);
        Task UpdateUbicacionAsync(int id, UbicacionDto ubicacionDto);
        Task DeleteUbicacionAsync(int id);
    }
}
