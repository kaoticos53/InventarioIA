using Inventario.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> GetAllUsuariosAsync();
        Task<UsuarioDto> GetUsuarioByIdAsync(string id);
        Task<UsuarioDto> CreateUsuarioAsync(UsuarioDto usuarioDto);
        Task UpdateUsuarioAsync(string id, UsuarioDto usuarioDto);
        Task DeleteUsuarioAsync(string id);
    }
}
