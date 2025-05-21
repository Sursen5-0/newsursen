using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class EmployeeDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public Guid? SeveraId { get; set; }
    }
}
