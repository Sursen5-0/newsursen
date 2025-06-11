using Hangfire.Jobs;

namespace Hangfire
{
    public static class JobRegistryExtensions
    {
        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddScoped<SeveraJobs>();
            services.AddScoped<EntraJobs>();
            services.AddScoped<FlowCaseJobs>();
            services.AddScoped<ReccuringJobRegistry>();
        }
    }
}
