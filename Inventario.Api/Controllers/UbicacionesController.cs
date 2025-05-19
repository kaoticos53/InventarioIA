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
    public class UbicacionesController : ControllerBase
    {
        private readonly IUbicacionService _ubicacionService;
        private readonly ILogger<UbicacionesController> _logger;

        public UbicacionesController(
            IUbicacionService ubicacionService,
            ILogger<UbicacionesController> logger)
        {
            _ubicacionService = ubicacionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las ubicaciones
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UbicacionDto>>> GetUbicaciones()
        {
            try
            {
                var ubicaciones = await _ubicacionService.GetAllUbicacionesAsync();
                return Ok(ubicaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las ubicaciones");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener las ubicaciones" });
            }
        }

        /// <summary>
        /// Obtiene las ubicaciones activas
        /// </summary>
        [HttpGet("activas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UbicacionDto>>> GetUbicacionesActivas()
        {
            try
            {
                var ubicaciones = await _ubicacionService.GetUbicacionesActivasAsync();
                return Ok(ubicaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las ubicaciones activas");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener las ubicaciones activas" });
            }
        }

        /// <summary>
        /// Obtiene una ubicación por su ID
        /// </summary>
        /// <param name="id">ID de la ubicación</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UbicacionDto>> GetUbicacion(int id)
        {
            try
            {
                var ubicacion = await _ubicacionService.GetUbicacionByIdAsync(id);
                return Ok(ubicacion);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ubicación no encontrada con ID: {UbicacionId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la ubicación con ID: {UbicacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al obtener la ubicación" });
            }
        }

        /// <summary>
        /// Crea una nueva ubicación
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<ActionResult<UbicacionDto>> CreateUbicacion(CreateUbicacionDto createUbicacionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var ubicacion = await _ubicacionService.CreateUbicacionAsync(createUbicacionDto);
                return CreatedAtAction(nameof(GetUbicacion), new { id = ubicacion.Id }, ubicacion);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear la ubicación");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la ubicación");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al crear la ubicación" });
            }
        }

        /// <summary>
        /// Actualiza una ubicación existente
        /// </summary>
        /// <param name="id">ID de la ubicación a actualizar</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> UpdateUbicacion(int id, UpdateUbicacionDto updateUbicacionDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID de ubicación no válido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _ubicacionService.UpdateUbicacionAsync(id, updateUbicacionDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ubicación no encontrada con ID: {UbicacionId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar la ubicación con ID: {UbicacionId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la ubicación con ID: {UbicacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al actualizar la ubicación" });
            }
        }

        /// <summary>
        /// Elimina una ubicación
        /// </summary>
        /// <param name="id">ID de la ubicación a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize(Roles = "Administrador")]  // Temporalmente deshabilitado para pruebas
        public async Task<IActionResult> DeleteUbicacion(int id)
        {
            try
            {
                await _ubicacionService.DeleteUbicacionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ubicación no encontrada con ID: {UbicacionId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No se puede eliminar la ubicación con ID: {UbicacionId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la ubicación con ID: {UbicacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al eliminar la ubicación" });
            }
        }
    }
}
