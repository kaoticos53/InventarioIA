using AutoMapper;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Domain.Entities;

namespace Inventario.Core.Application.Common.Mapping
{
    public class EquipoProfile : Profile
    {
        public EquipoProfile()
        {
            CreateMap<Equipo, EquipoDto>()
                .ForMember(dest => dest.UbicacionNombre, opt => opt.MapFrom(src => src.Ubicacion.Nombre))
                .ForMember(dest => dest.UsuarioCreacion, opt => opt.MapFrom(src => src.UsuarioCreacion != null ? $"{src.UsuarioCreacion.Nombre} {src.UsuarioCreacion.Apellido}" : null));
            
            CreateMap<CreateEquipoDto, Equipo>();
            CreateMap<UpdateEquipoDto, Equipo>();
        }
    }
}
