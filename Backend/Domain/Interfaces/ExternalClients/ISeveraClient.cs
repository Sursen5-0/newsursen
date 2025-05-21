using Domain.Models;

namespace Domain.Interfaces.ExternalClients
{
    public interface ISeveraClient
    {
        Task<string> GetToken();
        Task<EmployeeContractDTO> GetWorkContractByUserId(Guid userId);
    }
}