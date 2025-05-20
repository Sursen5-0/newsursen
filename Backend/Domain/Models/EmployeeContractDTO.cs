using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class EmployeeContractDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public decimal ExpectedHours { get; set; }
        public Guid SeveraId { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
