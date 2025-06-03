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
        Task<IEnumerable<SkillDTO>> GetAllSkillsAsync();
        Task AddSkillAsync(IEnumerable<SkillDTO> skills);
    }
}
