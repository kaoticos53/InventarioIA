using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventario.Core;
using Inventario.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inventario.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EquiposController : ControllerBase
    {
        private readonly IEquipoService _equipoService;
        private readonly ILogger<EquiposController> _logger;

        public EquiposController(IEquipoService equipoService, ILogger<EquiposController> logger)
        {
            _equipoService = equipoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los equipos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<EquipoDto>>> GetEquipos()
        {
            try
            {
                var equipos = await _equipoService.GetAllEquiposAsync();
                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los equipos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un equipo por su ID
        /// </summary>
        /// <param name="id">ID del equipo</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EquipoDto>> GetEquipo(int id)
        {
            try
            {
                var equipo = await _equipoService.GetEquipoByIdAsync(id);
                return Ok(equipo);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Equipo no encontrado con ID: {EquipoId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el equipo con ID: {EquipoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo equipo
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<EquipoDto>> CreateEquipo(CreateEquipoDto createEquipoDto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var equipo = await _equipoService.CreateEquipoAsync(createEquipoDto, userId);
                return CreatedAtAction(nameof(GetEquipo), new { id = equipo.Id }, equipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el equipo");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un equipo existente
        /// </summary>
        /// <param name="id">ID del equipo a actualizar</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> UpdateEquipo(int id, UpdateEquipoDto updateEquipoDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID de equipo no válido" });
            }

            try
            {
                await _equipoService.UpdateEquipoAsync(id, updateEquipoDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Equipo no encontrado con ID: {EquipoId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el equipo con ID: {EquipoId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un equipo
        /// </summary>
        /// <param name="id">ID del equipo a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteEquipo(int id)
        {
            try
            {
                await _equipoService.DeleteEquipoAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Equipo no encontrado con ID: {EquipoId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el equipo con ID: {EquipoId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los equipos por ubicación
        /// </summary>
        /// <param name="ubicacionId">ID de la ubicación</param>
        [HttpGet("por-ubicacion/{ubicacionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<EquipoDto>>> GetEquiposPorUbicacion(int ubicacionId)
        {
            try
            {
                var equipos = await _equipoService.GetEquiposByUbicacionAsync(ubicacionId);
                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los equipos por ubicación: {UbicacionId}", ubicacionId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene los equipos por estado
        /// </summary>
        /// <param name="estado">Estado del equipo (Disponible, En Uso, En Mantenimiento, Baja)</param>
        [HttpGet("por-estado/{estado}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<EquipoDto>>> GetEquiposPorEstado(string estado)
        {
            try
            {
                var equipos = await _equipoService.GetEquiposByEstadoAsync(estado);
                return Ok(equipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los equipos por estado: {Estado}", estado);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
