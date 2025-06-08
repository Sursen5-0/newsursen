using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Services
{
    public interface ISkillService
    {
        /// <summary>
        /// Synchronizes the skills table with the latest skills from Flowcase.
        /// Retrieves all skills from the Flowcase API, compares them with existing skills in the database,
        /// and adds any new skills that do not already exist. Existing skills are not updated or duplicated.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public Task SynchronizeSkillsFromFlowcaseAsync();
    }
}
