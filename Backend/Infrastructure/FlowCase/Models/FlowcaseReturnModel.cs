using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FlowCase.Models
{
    public class FlowcaseReturnModel<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }
}
