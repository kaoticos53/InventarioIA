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
    public class UbicacionService : IUbicacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UbicacionService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UbicacionDto>> GetAllUbicacionesAsync()
        {
            var ubicaciones = await _context.Ubicaciones
                .AsNoTracking()
                .ToListAsync();

            var ubicacionesDto = _mapper.Map<List<UbicacionDto>>(ubicaciones);
            
            // Contar equipos por ubicación
            foreach (var ubicacion in ubicacionesDto)
            {
                ubicacion.TotalEquipos = await _context.Equipos
                    .CountAsync(e => e.UbicacionId == ubicacion.Id);
            }

            return ubicacionesDto;
        }

        public async Task<UbicacionDto> GetUbicacionByIdAsync(int id)
        {
            var ubicacion = await _context.Ubicaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ubicacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la ubicación con ID {id}");
            }

            var ubicacionDto = _mapper.Map<UbicacionDto>(ubicacion);
            ubicacionDto.TotalEquipos = await _context.Equipos
                .CountAsync(e => e.UbicacionId == id);

            return ubicacionDto;
        }

        public async Task<UbicacionDto> CreateUbicacionAsync(CreateUbicacionDto ubicacionDto)
        {
            // Verificar si ya existe una ubicación con el mismo nombre
            var existe = await _context.Ubicaciones
                .AnyAsync(u => u.Nombre.ToLower() == ubicacionDto.Nombre.ToLower());

            if (existe)
            {
                throw new InvalidOperationException($"Ya existe una ubicación con el nombre {ubicacionDto.Nombre}");
            }

            var ubicacion = _mapper.Map<Ubicacion>(ubicacionDto);
            _context.Ubicaciones.Add(ubicacion);
            await _context.SaveChangesAsync();

            return _mapper.Map<UbicacionDto>(ubicacion);
        }

        public async Task UpdateUbicacionAsync(int id, UpdateUbicacionDto ubicacionDto)
        {
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la ubicación con ID {id}");
            }

            // Verificar si el nuevo nombre ya está en uso por otra ubicación
            if (await _context.Ubicaciones.AnyAsync(u => 
                u.Id != id && u.Nombre.ToLower() == ubicacionDto.Nombre.ToLower()))
            {
                throw new InvalidOperationException($"Ya existe otra ubicación con el nombre {ubicacionDto.Nombre}");
            }

            _mapper.Map(ubicacionDto, ubicacion);
            _context.Ubicaciones.Update(ubicacion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUbicacionAsync(int id)
        {
            var ubicacion = await _context.Ubicaciones
                .Include(u => u.Equipos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ubicacion == null)
            {
                throw new KeyNotFoundException($"No se encontró la ubicación con ID {id}");
            }

            if (ubicacion.Equipos != null && ubicacion.Equipos.Any())
            {
                throw new InvalidOperationException("No se puede eliminar una ubicación que tiene equipos asignados");
            }

            _context.Ubicaciones.Remove(ubicacion);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UbicacionDto>> GetUbicacionesActivasAsync()
        {
            var ubicaciones = await _context.Ubicaciones
                .Where(u => u.Activo)
                .AsNoTracking()
                .ToListAsync();

            var ubicacionesDto = _mapper.Map<List<UbicacionDto>>(ubicaciones);
            
            // Contar equipos por ubicación
            foreach (var ubicacion in ubicacionesDto)
            {
                ubicacion.TotalEquipos = await _context.Equipos
                    .CountAsync(e => e.UbicacionId == ubicacion.Id);
            }

            return ubicacionesDto;
        }
    }
}
