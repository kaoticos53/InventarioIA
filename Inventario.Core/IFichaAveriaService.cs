using Inventario.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core
{
    public interface IFichaAveriaService
    {
        Task<IEnumerable<FichaAveriaDto>> GetAllFichasAveriaAsync();
        Task<FichaAveriaDto> GetFichaAveriaByIdAsync(int id);
        Task<FichaAveriaDto> CreateFichaAveriaAsync(CreateFichaAveriaDto fichaAveriaDto, string usuarioReporteId);
        Task<FichaAveriaDto> UpdateFichaAveriaAsync(int id, UpdateFichaAveriaDto fichaAveriaDto);
        Task DeleteFichaAveriaAsync(int id);
        Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByEquipoIdAsync(int equipoId);
        Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByUsuarioReporteAsync(string usuarioId);
        Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByUsuarioAsignadoAsync(string usuarioId);
        Task<IEnumerable<FichaAveriaDto>> FilterFichasAveriaAsync(FichaAveriaFilterDto filter);
        Task<FichaAveriaDto> AsignarTecnicoAsync(int fichaAveriaId, string tecnicoId);
        Task<FichaAveriaDto> CambiarEstadoAsync(int fichaAveriaId, string nuevoEstado, string? solucion = null);
    }
}
