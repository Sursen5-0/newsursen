using Domain.Models;

namespace Domain.Interfaces.ExternalClients
{
    public interface ISeveraClient
    {
        Task<string> GetToken();
        Task<EmployeeContractDTO> GetWorkContractByUserId(Guid userId);
        Task<SeveraEmployeeModel?> GetUserByEmail(string emails);
        Task<IEnumerable<AbsenceDTO>> GetAbsence();
        Task<IEnumerable<ProjectDTO>> GetProjects();
        Task<IEnumerable<ProjectPhaseDTO>> GetPhases(IEnumerable<Guid> projectIds);
    }
}