using AutoMapper;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Domain.Entities;

namespace Inventario.Core.Application.Common.Mapping
{
    public class UbicacionProfile : Profile
    {
        public UbicacionProfile()
        {
            CreateMap<Ubicacion, UbicacionDto>()
                .ForMember(dest => dest.TotalEquipos, opt => opt.MapFrom(src => src.Equipos.Count(e => e.Activo)));
            
            CreateMap<CreateUbicacionDto, Ubicacion>();
            CreateMap<UpdateUbicacionDto, Ubicacion>();
        }
    }
}
