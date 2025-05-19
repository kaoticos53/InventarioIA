using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
    public class FichasAveriaController : ControllerBase
    {
        private readonly IFichaAveriaService _fichaAveriaService;
        private readonly ILogger<FichasAveriaController> _logger;

        public FichasAveriaController(
            IFichaAveriaService fichaAveriaService,
            ILogger<FichasAveriaController> logger)
        {
            _fichaAveriaService = fichaAveriaService;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Obtiene todas las fichas de avería
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FichaAveriaDto>>> GetFichasAveria()
        {
            try
            {
                var fichas = await _fichaAveriaService.GetAllFichasAveriaAsync();
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las fichas de avería");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener las fichas de avería" });
            }
        }

        /// <summary>
        /// Obtiene una ficha de avería por su ID
        /// </summary>
        /// <param name="id">ID de la ficha de avería</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FichaAveriaDto>> GetFichaAveria(int id)
        {
            try
            {
                var ficha = await _fichaAveriaService.GetFichaAveriaByIdAsync(id);
                return Ok(ficha);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ficha de avería no encontrada con ID: {FichaId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la ficha de avería con ID: {FichaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener la ficha de avería" });
            }
        }

        /// <summary>
        /// Crea una nueva ficha de avería
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FichaAveriaDto>> CreateFichaAveria(CreateFichaAveriaDto createFichaAveriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioId = GetCurrentUserId();
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var ficha = await _fichaAveriaService.CreateFichaAveriaAsync(createFichaAveriaDto, usuarioId);
                return CreatedAtAction(nameof(GetFichaAveria), new { id = ficha.Id }, ficha);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear la ficha de avería");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la ficha de avería");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al crear la ficha de avería" });
            }
        }

        /// <summary>
        /// Actualiza una ficha de avería existente
        /// </summary>
        /// <param name="id">ID de la ficha de avería a actualizar</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor,Tecnico")]
        public async Task<ActionResult<FichaAveriaDto>> UpdateFichaAveria(int id, UpdateFichaAveriaDto updateFichaAveriaDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID de ficha de avería no válido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var ficha = await _fichaAveriaService.UpdateFichaAveriaAsync(id, updateFichaAveriaDto);
                return Ok(ficha);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ficha de avería no encontrada con ID: {FichaId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar la ficha de avería con ID: {FichaId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la ficha de avería con ID: {FichaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al actualizar la ficha de avería" });
            }
        }

        /// <summary>
        /// Elimina una ficha de avería
        /// </summary>
        /// <param name="id">ID de la ficha de avería a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> DeleteFichaAveria(int id)
        {
            try
            {
                await _fichaAveriaService.DeleteFichaAveriaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ficha de avería no encontrada con ID: {FichaId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la ficha de avería con ID: {FichaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al eliminar la ficha de avería" });
            }
        }

        /// <summary>
        /// Obtiene las fichas de avería por ID de equipo
        /// </summary>
        /// <param name="equipoId">ID del equipo</param>
        [HttpGet("por-equipo/{equipoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FichaAveriaDto>>> GetFichasPorEquipo(int equipoId)
        {
            if (equipoId <= 0)
            {
                return BadRequest(new { message = "ID de equipo no válido" });
            }

            try
            {
                var fichas = await _fichaAveriaService.GetFichasAveriaByEquipoIdAsync(equipoId);
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las fichas de avería para el equipo con ID: {EquipoId}", equipoId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener las fichas de avería del equipo" });
            }
        }

        /// <summary>
        /// Obtiene las fichas de avería reportadas por el usuario actual
        /// </summary>
        [HttpGet("mis-reportes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FichaAveriaDto>>> GetMisReportes()
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var fichas = await _fichaAveriaService.GetFichasAveriaByUsuarioReporteAsync(usuarioId);
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los reportes del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener los reportes" });
            }
        }

        /// <summary>
        /// Obtiene las fichas de avería asignadas al usuario actual (técnico)
        /// </summary>
        [HttpGet("mis-asignaciones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor,Tecnico")]
        public async Task<ActionResult<IEnumerable<FichaAveriaDto>>> GetMisAsignaciones()
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var fichas = await _fichaAveriaService.GetFichasAveriaByUsuarioAsignadoAsync(usuarioId);
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las asignaciones del técnico");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener las asignaciones" });
            }
        }

        /// <summary>
        /// Filtra las fichas de avería según los criterios especificados
        /// </summary>
        [HttpPost("filtrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<IEnumerable<FichaAveriaDto>>> FiltrarFichas(FichaAveriaFilterDto filtro)
        {
            try
            {
                var fichas = await _fichaAveriaService.FilterFichasAveriaAsync(filtro);
                return Ok(fichas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar las fichas de avería");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al filtrar las fichas de avería" });
            }
        }

        /// <summary>
        /// Asigna un técnico a una ficha de avería
        /// </summary>
        /// <param name="id">ID de la ficha de avería</param>
        /// <param name="tecnicoId">ID del técnico a asignar</param>
        [HttpPost("{id}/asignar-tecnico/{tecnicoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<FichaAveriaDto>> AsignarTecnico(int id, string tecnicoId)
        {
            try
            {
                var ficha = await _fichaAveriaService.AsignarTecnicoAsync(id, tecnicoId);
                return Ok(ficha);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ficha de avería o técnico no encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al asignar técnico");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar técnico a la ficha de avería con ID: {FichaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al asignar técnico" });
            }
        }

        /// <summary>
        /// Cambia el estado de una ficha de avería
        /// </summary>
        /// <param name="id">ID de la ficha de avería</param>
        /// <param name="estado">Nuevo estado (Reportada, En Proceso, Resuelta, Cancelada)</param>
        /// <param name="solucion">Solución aplicada (opcional)</param>
        [HttpPost("{id}/cambiar-estado/{estado}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrador,Supervisor,Tecnico")]
        public async Task<ActionResult<FichaAveriaDto>> CambiarEstado(
            int id, 
            string estado, 
            [FromQuery] string? solucion = null)
        {
            try
            {
                var ficha = await _fichaAveriaService.CambiarEstadoAsync(id, estado, solucion);
                return Ok(ficha);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ficha de avería no encontrada con ID: {FichaId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado de la ficha de avería con ID: {FichaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al cambiar el estado" });
            }
        }
    }
}
