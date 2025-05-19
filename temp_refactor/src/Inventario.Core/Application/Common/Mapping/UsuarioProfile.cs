using AutoMapper;
using Inventario.Core.Application.DTOs;
using Inventario.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Inventario.Core.Application.Common.Mapping
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<ApplicationUser, UsuarioDto>()
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}".Trim()));
            
            CreateMap<CreateUsuarioDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()));
            
            CreateMap<UpdateUsuarioDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()));
            
            CreateMap<UpdateUsuarioPerfilDto, ApplicationUser>();
        }
    }
}
