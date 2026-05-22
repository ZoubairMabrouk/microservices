using MdxServices.Interfaces;
using MdxServices.MDX;
using MdxServices.Services;

namespace MdxServices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMdxServices(this IServiceCollection services)
        {
            services.AddScoped<IMdxQuery, MdxQuery>();
            services.AddScoped<IMdxService, MdxService>();
            return services;
        }
    }
}
