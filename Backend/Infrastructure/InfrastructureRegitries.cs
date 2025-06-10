using Application.Services;
using Domain.Interfaces.ExternalClients;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Infrastructure.Common;
using Infrastructure.Entra;
using Infrastructure.FlowCase;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Secrets;
using Infrastructure.Severa;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class InfrastructureRegitries
    {
        public static void RegisterInfrastructureServices(this IServiceCollection services, string token, string environment)
        {
            services.AddHttpClient();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

            services.AddDbContext<SursenContext>((services, options) =>
            {
                var secretClient = services.GetRequiredService<ISecretClient>();
                var connectionString = secretClient.GetSecretAsync("CONNECTIONSTRING").Result;
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly("Infrastructure"));
            });

            services.AddScoped<ISecretClient, DopplerClient>(provider =>
            {
                var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = clientFactory.CreateClient("doppler");
                httpClient.BaseAddress = new Uri("https://api.doppler.com/v3/");
                return new DopplerClient(httpClient, token, environment);
            });

            services.AddHttpClient<IEntraClient, EntraClient>()
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
                    return new RetryHandler(new HttpClientHandler(), logger);
                }); 
            services.AddHttpClient<IFlowCaseClient, FlowCaseClient>(client =>
            {
                client.BaseAddress = new Uri("https://twoday.flowcase.com");
            })
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
                    return new RetryHandler(new HttpClientHandler(), logger);
                });

            services.AddHttpClient<ISeveraClient, SeveraClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.severa.visma.com/rest-api/v1.0/");
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RetryHandler>>();
                return new RetryHandler(new HttpClientHandler(), logger);
            });

        }
    }
}
