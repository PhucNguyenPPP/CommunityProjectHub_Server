using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;
using Microsoft.Identity.Client;

namespace CPH.Common.DTO.Registration
{
    public class ListSentRegisDTO
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<SentRegistDTO>? Registered { get; set; }
    }
}
