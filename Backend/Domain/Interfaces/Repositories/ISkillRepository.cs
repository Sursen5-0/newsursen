using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
namespace Domain.Interfaces.Repositories
{
    public interface ISkillRepository
    {
        /// <summary>
        /// Adds a collection of new skills to the database.
        /// </summary>
        /// <param name="skills">A collection of <see cref="SkillDTO"/> objects to be added.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task<IEnumerable<SkillDTO>> GetAllSkillsAsync();
        /// <summary>
        /// Retrieves all skills from the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a collection of <see cref="SkillDTO"/> objects.
        /// </returns>
        Task AddSkillAsync(IEnumerable<SkillDTO> skills);
    }
}
