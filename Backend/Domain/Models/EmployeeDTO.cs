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

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public DateTime? HireDate { get; set; }

        public DateOnly? LeaveDate { get; set; }

        public DateOnly Birthdate { get; set; }

        public Guid BusinessUnitId { get; set; }

        public string? WorkPhoneNumber { get; set; }

        public string? PersonalPhoneNumber { get; set; }

        public Guid? SeveraId { get; set; }

        public Guid EntraId { get; set; }

        public string? FlowCaseId { get; set; }
        public string? CvId { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? EntraManagerId { get; set; }
        public string? UserPrincipalName { get; set; }

    }
}
