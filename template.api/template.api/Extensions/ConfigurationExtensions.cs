using Microsoft.Extensions.DependencyInjection;
using template.api.ActionFilter;
using template.biz.Servicies;
using template.biz.Repository;
using template.dal.Repository;
using mieleApp.biz.Repository.Product;
using mieleApp.dal.Repository.Product;
using mieleApp.biz.Repository.Address;
using mieleApp.dal.Repository.Address;
using mieleApp.biz.Repository.Service;
using mieleApp.dal.Repository.Service;

namespace template.api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IProductUserRepository, ProductUserRepository>();
            services.AddTransient<IProductUserRepository, ProductUserRepository>();
            services.AddTransient<IPlaceRepository, PlaceRepository>();
            services.AddTransient<ITownRepository, TownRepository>();
            services.AddTransient<IStateRepository, StateRepository>();
            services.AddTransient<IAddressRepository, CatAddressRepository>();
            services.AddTransient<IServiceTypeRepository, ServiceTypeRepository>();
            services.AddTransient<IServiceRepository, ServiceRepository>();

        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureFilters(this IServiceCollection services)
        {
            services.AddScoped<ValidationFilterAttribute>();
        }
    }
}
