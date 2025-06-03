using Domain.Models;

namespace Domain.Interfaces.ExternalClients
{
    public interface ISeveraClient
    {
        Task<string> GetToken();
        Task<EmployeeContractDTO?> GetWorkContractByUserId(Guid userId);
        Task<SeveraEmployeeModel?> GetUserByEmail(string emails);
        Task<IEnumerable<AbsenceDTO>> GetAbsence(DateTime? changedDate = null, int? maxpages = null);
        Task<IEnumerable<ProjectDTO>> GetProjects(DateTime? changedDate = null, int? maxpages = null);
        Task<IEnumerable<ProjectPhaseDTO>> GetPhases(IEnumerable<Guid> projectIds, DateTime? changedDate = null, int? maxpages = null);
    }
}