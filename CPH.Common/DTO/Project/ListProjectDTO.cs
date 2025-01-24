using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Paging;

namespace CPH.Common.DTO.Project
{
    public class ListProjectDTO
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<GetAllProjectDTO>? GetAllProjectDTOs { get; set; }
    }
}
