using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Inventario.Core;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Inventario.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventario.Infrastructure
{
    public class FichaAveriaService : IFichaAveriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FichaAveriaService> _logger;

        public FichaAveriaService(
            ApplicationDbContext context, 
            IMapper mapper,
            ILogger<FichaAveriaService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<FichaAveriaDto>> GetAllFichasAveriaAsync()
        {
            var fichas = await _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioResolucion)
                .AsNoTracking()
                .OrderByDescending(f => f.FechaReporte)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichaAveriaDto>>(fichas);
        }

        public async Task<FichaAveriaDto> GetFichaAveriaByIdAsync(int id)
        {
            var ficha = await _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioResolucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            if (ficha == null)
            {
                throw new KeyNotFoundException($"No se encontró la ficha de avería con ID {id}");
            }

            return _mapper.Map<FichaAveriaDto>(ficha);
        }

        public async Task<FichaAveriaDto> CreateFichaAveriaAsync(CreateFichaAveriaDto fichaAveriaDto, string usuarioReporteId)
        {
            // Verificar si el equipo existe
            var equipo = await _context.Equipos.FindAsync(fichaAveriaDto.EquipoId);
            if (equipo == null)
            {
                throw new InvalidOperationException($"No se encontró el equipo con ID {fichaAveriaDto.EquipoId}");
            }

            // Verificar si el usuario que reporta existe
            var usuarioReporte = await _context.Users.FindAsync(usuarioReporteId);
            if (usuarioReporte == null)
            {
                throw new InvalidOperationException($"No se encontró el usuario con ID {usuarioReporteId}");
            }

            var fichaAveria = _mapper.Map<FichaAveria>(fichaAveriaDto);
            fichaAveria.FechaReporte = DateTime.UtcNow;
            fichaAveria.Estado = "Reportada";
            fichaAveria.UsuarioReporteId = usuarioReporteId;

            _context.FichasAveria.Add(fichaAveria);
            await _context.SaveChangesAsync();

            // Actualizar el estado del equipo a "En Reparación"
            equipo.Estado = "En Reparación";
            _context.Equipos.Update(equipo);
            await _context.SaveChangesAsync();

            return await GetFichaAveriaByIdAsync(fichaAveria.Id);
        }

        public async Task<FichaAveriaDto> UpdateFichaAveriaAsync(int id, UpdateFichaAveriaDto fichaAveriaDto)
        {
            var fichaAveria = await _context.FichasAveria.FindAsync(id);
            if (fichaAveria == null)
            {
                throw new KeyNotFoundException($"No se encontró la ficha de avería con ID {id}");
            }

            // Mapear solo las propiedades que no son nulas
            if (fichaAveriaDto.Titulo != null) fichaAveria.Titulo = fichaAveriaDto.Titulo;
            if (fichaAveriaDto.Descripcion != null) fichaAveria.Descripcion = fichaAveriaDto.Descripcion;
            if (fichaAveriaDto.SolucionAplicada != null) fichaAveria.SolucionAplicada = fichaAveriaDto.SolucionAplicada;
            if (fichaAveriaDto.Comentarios != null) fichaAveria.Comentarios = fichaAveriaDto.Comentarios;
            if (fichaAveriaDto.Prioridad != null) fichaAveria.Prioridad = fichaAveriaDto.Prioridad;

            // Si se asigna un técnico, verificar que exista
            if (!string.IsNullOrEmpty(fichaAveriaDto.UsuarioAsignadoId))
            {
                var usuarioAsignado = await _context.Users.FindAsync(fichaAveriaDto.UsuarioAsignadoId);
                if (usuarioAsignado == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario asignado con ID {fichaAveriaDto.UsuarioAsignadoId}");
                }
                fichaAveria.UsuarioAsignadoId = fichaAveriaDto.UsuarioAsignadoId;
            }

            // Si se cambia el estado, actualizar fechas según corresponda
            if (!string.IsNullOrEmpty(fichaAveriaDto.Estado))
            {
                fichaAveria.Estado = fichaAveriaDto.Estado;
                
                if (fichaAveriaDto.Estado == "Resuelta" && fichaAveria.FechaResolucion == null)
                {
                    fichaAveria.FechaResolucion = DateTime.UtcNow;
                    
                    // Actualizar el estado del equipo a "Disponible"
                    var equipo = await _context.Equipos.FindAsync(fichaAveria.EquipoId);
                    if (equipo != null)
                    {
                        equipo.Estado = "Disponible";
                        _context.Equipos.Update(equipo);
                    }
                }
            }

            _context.FichasAveria.Update(fichaAveria);
            await _context.SaveChangesAsync();

            return await GetFichaAveriaByIdAsync(fichaAveria.Id);
        }

        public async Task DeleteFichaAveriaAsync(int id)
        {
            var fichaAveria = await _context.FichasAveria.FindAsync(id);
            if (fichaAveria == null)
            {
                throw new KeyNotFoundException($"No se encontró la ficha de avería con ID {id}");
            }

            _context.FichasAveria.Remove(fichaAveria);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByEquipoIdAsync(int equipoId)
        {
            var fichas = await _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioAsignado)
                .Where(f => f.EquipoId == equipoId)
                .OrderByDescending(f => f.FechaReporte)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichaAveriaDto>>(fichas);
        }

        public async Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByUsuarioReporteAsync(string usuarioId)
        {
            var fichas = await _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioAsignado)
                .Where(f => f.UsuarioReporteId == usuarioId)
                .OrderByDescending(f => f.FechaReporte)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichaAveriaDto>>(fichas);
        }

        public async Task<IEnumerable<FichaAveriaDto>> GetFichasAveriaByUsuarioAsignadoAsync(string usuarioId)
        {
            var fichas = await _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioAsignado)
                .Where(f => f.UsuarioAsignadoId == usuarioId)
                .OrderByDescending(f => f.FechaReporte)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichaAveriaDto>>(fichas);
        }

        public async Task<IEnumerable<FichaAveriaDto>> FilterFichasAveriaAsync(FichaAveriaFilterDto filter)
        {
            var query = _context.FichasAveria
                .Include(f => f.Equipo)
                .Include(f => f.UsuarioReporte)
                .Include(f => f.UsuarioAsignado)
                .AsQueryable();

            if (filter.EquipoId.HasValue)
                query = query.Where(f => f.EquipoId == filter.EquipoId.Value);

            if (!string.IsNullOrEmpty(filter.Estado))
                query = query.Where(f => f.Estado == filter.Estado);

            if (!string.IsNullOrEmpty(filter.UsuarioReporteId))
                query = query.Where(f => f.UsuarioReporteId == filter.UsuarioReporteId);

            if (!string.IsNullOrEmpty(filter.UsuarioAsignadoId))
                query = query.Where(f => f.UsuarioAsignadoId == filter.UsuarioAsignadoId);

            if (!string.IsNullOrEmpty(filter.Prioridad))
                query = query.Where(f => f.Prioridad == filter.Prioridad);

            if (filter.FechaInicio.HasValue)
                query = query.Where(f => f.FechaReporte >= filter.FechaInicio.Value);

            if (filter.FechaFin.HasValue)
                query = query.Where(f => f.FechaReporte <= filter.FechaFin.Value);

            if (filter.IncluirResueltas == false)
                query = query.Where(f => f.Estado != "Resuelta");

            var fichas = await query
                .OrderByDescending(f => f.FechaReporte)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<FichaAveriaDto>>(fichas);
        }

        public async Task<FichaAveriaDto> AsignarTecnicoAsync(int fichaAveriaId, string tecnicoId)
        {
            var fichaAveria = await _context.FichasAveria.FindAsync(fichaAveriaId);
            if (fichaAveria == null)
            {
                throw new KeyNotFoundException($"No se encontró la ficha de avería con ID {fichaAveriaId}");
            }

            var tecnico = await _context.Users.FindAsync(tecnicoId);
            if (tecnico == null)
            {
                throw new InvalidOperationException($"No se encontró el técnico con ID {tecnicoId}");
            }

            fichaAveria.UsuarioAsignadoId = tecnicoId;
            fichaAveria.Estado = "En Proceso";

            _context.FichasAveria.Update(fichaAveria);
            await _context.SaveChangesAsync();

            return await GetFichaAveriaByIdAsync(fichaAveria.Id);
        }

        public async Task<FichaAveriaDto> CambiarEstadoAsync(int fichaAveriaId, string nuevoEstado, string? solucion = null)
        {
            var fichaAveria = await _context.FichasAveria
                .Include(f => f.Equipo)
                .FirstOrDefaultAsync(f => f.Id == fichaAveriaId);
                
            if (fichaAveria == null)
            {
                throw new KeyNotFoundException($"No se encontró la ficha de avería con ID {fichaAveriaId}");
            }

            fichaAveria.Estado = nuevoEstado;
            
            if (!string.IsNullOrEmpty(solucion))
            {
                fichaAveria.SolucionAplicada = solucion;
            }

            if (nuevoEstado == "Resuelta" && fichaAveria.FechaResolucion == null)
            {
                fichaAveria.FechaResolucion = DateTime.UtcNow;
                
                // Actualizar el estado del equipo a "Disponible"
                if (fichaAveria.Equipo != null)
                {
                    fichaAveria.Equipo.Estado = "Disponible";
                    _context.Equipos.Update(fichaAveria.Equipo);
                }
            }
            else if (nuevoEstado == "En Proceso" && fichaAveria.Equipo != null)
            {
                fichaAveria.Equipo.Estado = "En Reparación";
                _context.Equipos.Update(fichaAveria.Equipo);
            }

            _context.FichasAveria.Update(fichaAveria);
            await _context.SaveChangesAsync();

            return await GetFichaAveriaByIdAsync(fichaAveria.Id);
        }
    }
}
