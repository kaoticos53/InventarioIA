using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IFichaAveriaService
    {
        Task<IEnumerable<FichaAveriaDto>> GetAllFichasAveriaAsync();
        Task<FichaAveriaDto> GetFichaAveriaByIdAsync(int id);
        Task<FichaAveriaDto> CreateFichaAveriaAsync(FichaAveriaDto fichaAveriaDto, string userId);
        Task UpdateFichaAveriaAsync(int id, FichaAveriaDto fichaAveriaDto);
        Task DeleteFichaAveriaAsync(int id);
    }
}
