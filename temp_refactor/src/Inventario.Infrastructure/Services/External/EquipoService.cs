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

namespace Inventario.Infrastructure
{
    public class EquipoService : IEquipoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EquipoService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EquipoDto>> GetAllEquiposAsync()
        {
            var equipos = await _context.Equipos
                .Include(e => e.Ubicacion)
                .Include(e => e.UsuarioCreacion)
                .Include(e => e.FichasAveria)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<EquipoDto>>(equipos);
        }

        public async Task<EquipoDto> GetEquipoByIdAsync(int id)
        {
            var equipo = await _context.Equipos
                .Include(e => e.Ubicacion)
                .Include(e => e.UsuarioCreacion)
                .Include(e => e.FichasAveria)
                    .ThenInclude(f => f.UsuarioReporte)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipo == null)
            {
                throw new KeyNotFoundException($"No se encontró el equipo con ID {id}");
            }

            return _mapper.Map<EquipoDto>(equipo);
        }

        public async Task<EquipoDto> CreateEquipoAsync(CreateEquipoDto equipoDto, string userId)
        {
            var equipo = _mapper.Map<Equipo>(equipoDto);
            equipo.UsuarioCreacionId = userId;
            equipo.FechaCreacion = DateTime.UtcNow;

            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return await GetEquipoByIdAsync(equipo.Id);
        }

        public async Task UpdateEquipoAsync(int id, UpdateEquipoDto equipoDto)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                throw new KeyNotFoundException($"No se encontró el equipo con ID {id}");
            }

            _mapper.Map(equipoDto, equipo);
            equipo.FechaActualizacion = DateTime.UtcNow;

            _context.Equipos.Update(equipo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEquipoAsync(int id)
        {
            var equipo = await _context.Equipos.FindAsync(id);
            if (equipo == null)
            {
                throw new KeyNotFoundException($"No se encontró el equipo con ID {id}");
            }

            _context.Equipos.Remove(equipo);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EquipoDto>> GetEquiposByUbicacionAsync(int ubicacionId)
        {
            var equipos = await _context.Equipos
                .Include(e => e.Ubicacion)
                .Include(e => e.UsuarioCreacion)
                .Where(e => e.UbicacionId == ubicacionId)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<EquipoDto>>(equipos);
        }

        public async Task<IEnumerable<EquipoDto>> GetEquiposByEstadoAsync(string estado)
        {
            var equipos = await _context.Equipos
                .Include(e => e.Ubicacion)
                .Include(e => e.UsuarioCreacion)
                .Where(e => e.Estado == estado)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<EquipoDto>>(equipos);
        }
    }
}
