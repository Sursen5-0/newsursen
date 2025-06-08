using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.ExternalClients
{
    public interface IFlowCaseClient
    {
        /// <summary>
        /// Retrieves a list of skills from the FlowCase API and inserts them into the Skills table.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a list of <see cref="SkillDTO"/>
        /// objects representing the skills retrieved from the FlowCase API.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the API request fails or returns an unsuccessful response.
        /// </exception>
        public Task<List<SkillDTO>> GetSkillsFromFlowcaseAsync();

        /// <summary>
        /// Retrieves a list of skills from a CV for a specified user and CV ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose CV is being accessed. Cannot be null or empty.</param>
        /// <param name="cvId">The unique identifier of the CV from which skills are to be retrieved. Cannot be null or empty.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of <see cref="EmployeeSkillDTO"/>
        /// objects representing the skills extracted from the CV.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the operation to retrieve skills fails, including details of the failure in the exception message.
        /// </exception>
        public Task<List<EmployeeSkillDTO>> GetSkillsFromCVAsync(string userId, string cvId);

        /// <summary>
        /// Retrieves a list of users from the FlowCase API.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains a list of <see cref="FlowcaseUserModel"/>
        /// objects representing the users retrieved from the FlowCase API.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the API request fails or returns an unsuccessful response.
        /// </exception>
        public Task<List<FlowcaseUserModel>> GetUsersAsync();
    }
}
