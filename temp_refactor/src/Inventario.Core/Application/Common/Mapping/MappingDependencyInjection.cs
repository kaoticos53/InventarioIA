using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Inventario.Core.Application.Common.Mapping
{
    public static class MappingDependencyInjection
    {
        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
