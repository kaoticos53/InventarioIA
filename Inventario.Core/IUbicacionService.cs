using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core
{
    public interface IUbicacionService
    {
        Task<IEnumerable<UbicacionDto>> GetAllUbicacionesAsync();
        Task<UbicacionDto> GetUbicacionByIdAsync(int id);
        Task<UbicacionDto> CreateUbicacionAsync(CreateUbicacionDto ubicacionDto);
        Task UpdateUbicacionAsync(int id, UpdateUbicacionDto ubicacionDto);
        Task DeleteUbicacionAsync(int id);
        Task<IEnumerable<UbicacionDto>> GetUbicacionesActivasAsync();
    }
}
