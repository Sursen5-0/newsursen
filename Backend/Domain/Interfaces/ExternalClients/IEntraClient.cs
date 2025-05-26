using System.Threading.Tasks;

namespace Domain.Interfaces.ExternalClients
{
    public interface IEntraClient
    {
        Task<string?> GetTokenAsync();

        Task<string?> GetUsersJsonAsync();
    }
}
