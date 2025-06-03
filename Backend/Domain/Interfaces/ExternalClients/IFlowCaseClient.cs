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
        public Task<List<SkillDTO>> GetSkillsFromFlowcaseAsync();
        public Task<List<SkillDTO>> GetSkillsFromCVAsync(string userId, string cvId);
        public Task<List<FlowcaseUserModel>> GetUsersAsync();
    }
}
