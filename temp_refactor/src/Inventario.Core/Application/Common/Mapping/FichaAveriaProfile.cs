using AutoMapper;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Domain.Entities;

namespace Inventario.Core.Application.Common.Mapping
{
    public class FichaAveriaProfile : Profile
    {
        public FichaAveriaProfile()
        {
            CreateMap<FichaAveria, FichaAveriaDto>()
                .ForMember(dest => dest.EquipoNombre, opt => opt.MapFrom(src => src.Equipo.Nombre))
                .ForMember(dest => dest.UsuarioReporteNombre, opt => opt.MapFrom(src => 
                    src.UsuarioReporte != null ? $"{src.UsuarioReporte.Nombre} {src.UsuarioReporte.Apellido}" : null))
                .ForMember(dest => dest.UsuarioResolucionNombre, opt => opt.MapFrom(src => 
                    src.UsuarioResolucion != null ? $"{src.UsuarioResolucion.Nombre} {src.UsuarioResolucion.Apellido}" : null))
                .ForMember(dest => dest.UsuarioAsignadoNombre, opt => opt.MapFrom(src => 
                    src.UsuarioAsignado != null ? $"{src.UsuarioAsignado.Nombre} {src.UsuarioAsignado.Apellido}" : null));
            
            CreateMap<CreateFichaAveriaDto, FichaAveria>();
            CreateMap<UpdateFichaAveriaDto, FichaAveria>();
        }
    }
}
