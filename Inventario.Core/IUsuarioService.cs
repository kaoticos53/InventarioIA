using Inventario.Core.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventario.Core
{
    public interface IUsuarioService
    {
        // Usuarios
        Task<UsuarioDto> GetUsuarioByIdAsync(string id);
        Task<UsuarioDto> GetUsuarioByEmailAsync(string email);
        Task<UsuarioPaginacionDto> GetUsuariosAsync(UsuarioFiltroDto filtro);
        Task<IdentityResult> CreateUsuarioAsync(CreateUsuarioDto usuarioDto);
        Task<IdentityResult> UpdateUsuarioAsync(string id, UpdateUsuarioDto usuarioDto);
        Task<IdentityResult> DeleteUsuarioAsync(string id);
        
        // Roles
        Task<IList<string>> GetRolesAsync(string userId);
        Task<IdentityResult> AsignarRolAsync(string userId, string roleName);
        Task<IdentityResult> QuitarRolAsync(string userId, string roleName);
        Task<bool> IsInRoleAsync(string userId, string roleName);
        
        // Contraseñas
        Task<IdentityResult> CambiarContrasenaAsync(string userId, string currentPassword, string newPassword);
        Task<string> GenerarPasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetearContrasenaAsync(string email, string token, string newPassword);
        
        // Bloqueo/Desbloqueo
        Task<IdentityResult> BloquearUsuarioAsync(string userId, DateTime? fechaFinBloqueo = null);
        Task<IdentityResult> DesbloquearUsuarioAsync(string userId);
        
        // Confirmación de correo
        Task<string> GenerarEmailConfirmationTokenAsync(string email);
        Task<IdentityResult> ConfirmarEmailAsync(string userId, string token);
        
        // Autenticación
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeTokenAsync(string token);
    }
}
