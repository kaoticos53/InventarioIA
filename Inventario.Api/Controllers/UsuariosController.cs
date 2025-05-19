using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Inventario.Core;
using Inventario.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inventario.Api.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuariosController(
            IUsuarioService usuarioService,
            ILogger<UsuariosController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _usuarioService = usuarioService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Obtiene todos los usuarios con paginación
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioPaginacionDto>> GetUsuarios(
            [FromQuery] string busqueda = "",
            [FromQuery] bool? activo = null,
            [FromQuery] string rol = "",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 10)
        {
            try
            {
                var filtro = new UsuarioFiltroDto
                {
                    Busqueda = busqueda,
                    Activo = activo,
                    Rol = rol,
                    Pagina = pagina,
                    TamanioPagina = tamanioPagina
                };

                var resultado = await _usuarioService.GetUsuariosAsync(filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al obtener los usuarios" });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado con ID: {UsuarioId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {UsuarioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al obtener el usuario" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(CreateUsuarioDto createUsuarioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _usuarioService.CreateUsuarioAsync(createUsuarioDto);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByEmailAsync(createUsuarioDto.Email);
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al crear el usuario" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> UpdateUsuario(string id, UpdateUsuarioDto updateUsuarioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _usuarioService.UpdateUsuarioAsync(id, updateUsuarioDto);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado con ID: {UsuarioId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID: {UsuarioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al actualizar el usuario" });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            try
            {
                var result = await _usuarioService.DeleteUsuarioAsync(id);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado con ID: {UsuarioId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID: {UsuarioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al eliminar el usuario" });
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="rol">Nombre del rol a asignar</param>
        [HttpPost("{id}/roles/{rol}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> AsignarRol(string id, string rol)
        {
            try
            {
                var result = await _usuarioService.AsignarRolAsync(id, rol);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario o rol no encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar el rol {Rol} al usuario con ID: {UsuarioId}", rol, id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al asignar el rol" });
            }
        }

        /// <summary>
        /// Quita un rol a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="rol">Nombre del rol a quitar</param>
        [HttpDelete("{id}/roles/{rol}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> QuitarRol(string id, string rol)
        {
            try
            {
                var result = await _usuarioService.QuitarRolAsync(id, rol);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario o rol no encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el rol {Rol} al usuario con ID: {UsuarioId}", rol, id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al quitar el rol" });
            }
        }

        /// <summary>
        /// Bloquea un usuario
        /// </summary>
        /// <param name="id">ID del usuario a bloquear</param>
        /// <param name="minutos">Minutos de bloqueo (opcional, si no se especifica es permanente)</param>
        [HttpPost("{id}/bloquear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> BloquearUsuario(string id, [FromQuery] int? minutos = null)
        {
            try
            {
                DateTimeOffset? lockoutEnd = null;
                if (minutos.HasValue)
                {
                    var lockoutDate = DateTime.UtcNow.AddMinutes(minutos.Value);
                    lockoutEnd = lockoutDate;
                }

                var result = await _usuarioService.BloquearUsuarioAsync(id, lockoutEnd?.UtcDateTime);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado con ID: {UsuarioId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear el usuario con ID: {UsuarioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al bloquear el usuario" });
            }
        }

        /// <summary>
        /// Desbloquea un usuario
        /// </summary>
        /// <param name="id">ID del usuario a desbloquear</param>
        [HttpPost("{id}/desbloquear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> DesbloquearUsuario(string id)
        {
            try
            {
                var result = await _usuarioService.DesbloquearUsuarioAsync(id);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado con ID: {UsuarioId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear el usuario con ID: {UsuarioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor al desbloquear el usuario" });
            }
        }
    }
}
