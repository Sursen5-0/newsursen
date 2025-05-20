using Infrastructure.Entra;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hangfire.Jobs
{
    public class FetchGraphUsersJob
    {
        private readonly EntraClient _entraClient;

        public FetchGraphUsersJob(EntraClient entraClient)
        {
            _entraClient = entraClient;
        }

        public async Task WriteGraphUsers()
        {
            var json = await _entraClient.GetUsersJsonAsync();
            Debug.WriteLine($"[FetchGraphUsersJob] Users JSON:\n{json}");
        }
    }
}
