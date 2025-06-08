using Domain.Models;
using System.Threading.Tasks;

namespace Domain.Interfaces.ExternalClients
{
    public interface IEntraClient
    {
        /// <summary>
        /// Asynchronously retrieves an access token for authenticating requests to the Entra (Microsoft Graph) API.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the access token as a <see cref="string"/> if successful; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if an unexpected error occurs during token acquisition.
        /// </exception>
        Task<string?> GetTokenAsync();

        /// <summary>
        /// Asynchronously retrieves all employees from the Entra (Microsoft Graph) API.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a list of <see cref="EmployeeDTO"/> objects.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an access token cannot be acquired.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown if the HTTP request to the Graph API fails.
        /// </exception>
        Task<List<EmployeeDTO>> GetAllEmployeesAsync();
    }
}
