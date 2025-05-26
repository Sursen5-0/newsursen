using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AbsenceDTO
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public Guid ExternalId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? SeveraEmployeeId { get; set; }

    }
}
