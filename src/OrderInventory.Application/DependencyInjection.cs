using Microsoft.Extensions.DependencyInjection;
using OrderInventory.Application.Interfaces;
using OrderInventory.Application.Services;
using System.Reflection;

namespace OrderInventory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();


            return services;
        }
    }
}