using Application.Services;
using Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class ApplicationRegistries
    {
        public static void RegisterApplication(this IServiceCollection services)
        {
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
        }
    }
}
