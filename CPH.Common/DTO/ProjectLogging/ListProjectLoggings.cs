using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Paging;

namespace CPH.Common.DTO.ProjectLogging
{
    public class ListProjectLoggings
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<GetAllProjectLoggingDTO>? getAllProjectLoggingDTOs { get; set; }
    }
}
