using Domain.Models;

namespace Domain.Interfaces.ExternalClients
{
    public interface ISeveraClient
    {
        /// <summary>
        /// Retrieves an access token for authenticating requests to the Severa API.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the access token as a <see cref="string"/>.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP request to obtain the token fails.
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when the response cannot be deserialized into a <see cref="TokenResponse"/>.
        /// </exception>
        Task<string> GetToken();

        /// <summary>
        /// Retrieves the current work contract for a specific user from Severa.
        /// </summary>
        /// <param name="userId">
        /// The <see cref="Guid"/> representing the user's unique identifier.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with an <see cref="EmployeeContractDTO"/> if found; otherwise, <c>null</c>.
        /// </returns>
        Task<EmployeeContractDTO?> GetWorkContractByUserId(Guid userId);

        /// <summary>
        /// Retrieves a Severa user by their email address.
        /// </summary>
        /// <param name="email">
        /// The email address of the user to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="SeveraEmployeeModel"/> if found; otherwise, <c>null</c>.
        /// </returns>
        Task<SeveraEmployeeModel?> GetUserByEmail(string emails);

        /// <summary>
        /// Retrieves a collection of absences from Severa, optionally filtered by change date and paginated.
        /// </summary>
        /// <param name="changedDate">
        /// The optional <see cref="DateTime"/> to filter absences changed since this date.
        /// </param>
        /// <param name="maxpages">
        /// The optional maximum number of pages to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="AbsenceDTO"/>.
        /// </returns>
        Task<IEnumerable<AbsenceDTO>> GetAbsence(DateTime? changedDate = null, int? maxpages = null);

        /// <summary>
        /// Retrieves a collection of projects from Severa, optionally filtered by change date and paginated.
        /// </summary>
        /// <param name="changedDate">
        /// The optional <see cref="DateTime"/> to filter projects changed since this date.
        /// </param>
        /// <param name="maxpages">
        /// The optional maximum number of pages to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="ProjectDTO"/>.
        /// </returns>
        Task<IEnumerable<ProjectDTO>> GetProjects(DateTime? changedDate = null, int? maxpages = null);

        /// <summary>
        /// Retrieves a collection of project phases for the specified project IDs from Severa, optionally filtered by change date and paginated.
        /// </summary>
        /// <param name="projectIds">
        /// The collection of <see cref="Guid"/> representing the project IDs.
        /// </param>
        /// <param name="changedDate">
        /// The optional <see cref="DateTime"/> to filter phases changed since this date.
        /// </param>
        /// <param name="maxpages">
        /// The optional maximum number of pages to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="ProjectPhaseDTO"/>.
        /// </returns>
        Task<IEnumerable<ProjectPhaseDTO>> GetPhases(IEnumerable<Guid> projectIds, DateTime? changedDate = null, int? maxpages = null);
    }
}