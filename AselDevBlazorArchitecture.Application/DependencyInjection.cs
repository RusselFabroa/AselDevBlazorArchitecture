using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
        this IServiceCollection services)
        {
            // Register Application layer services here
            // Example: services.AddScoped<ISomeService, SomeService>();

            return services;
        }
    }
}
