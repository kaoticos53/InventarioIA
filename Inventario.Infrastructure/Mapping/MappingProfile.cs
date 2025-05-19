using AutoMapper;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Inventario.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos de Usuario
            CreateMap<ApplicationUser, UsuarioDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            CreateMap<CreateUsuarioDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true));

            CreateMap<UpdateUsuarioDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Mapeos de Roles
            CreateMap<RolPersonalizado, RolDto>();
            CreateMap<RolDto, RolPersonalizado>();

            // Mapeos de Equipo
            CreateMap<Equipo, EquipoDto>();
            CreateMap<EquipoDto, Equipo>();
            CreateMap<CreateEquipoDto, Equipo>();
            CreateMap<UpdateEquipoDto, Equipo>();

            // Mapeos de Ubicación
            CreateMap<Ubicacion, UbicacionDto>();
            CreateMap<UbicacionDto, Ubicacion>();
            CreateMap<CreateUbicacionDto, Ubicacion>();
            CreateMap<UpdateUbicacionDto, Ubicacion>();

            // Mapeos de Ficha de Avería
            CreateMap<FichaAveria, FichaAveriaDto>()
                .ForMember(dest => dest.UsuarioReporteNombre, opt => opt.MapFrom(src => src.UsuarioReporte != null ? $"{src.UsuarioReporte.Nombre} {src.UsuarioReporte.Apellido}" : null))
                .ForMember(dest => dest.UsuarioAsignadoNombre, opt => opt.MapFrom(src => src.UsuarioAsignado != null ? $"{src.UsuarioAsignado.Nombre} {src.UsuarioAsignado.Apellido}" : null))
                .ForMember(dest => dest.EquipoNombre, opt => opt.MapFrom(src => src.Equipo.Nombre));

            CreateMap<FichaAveriaDto, FichaAveria>();
            CreateMap<CreateFichaAveriaDto, FichaAveria>();
            CreateMap<UpdateFichaAveriaDto, FichaAveria>();
        }
    }
}
