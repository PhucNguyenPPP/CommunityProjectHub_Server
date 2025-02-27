using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Member;
using CPH.Common.DTO.Paging;

namespace CPH.Common.DTO.Lecturer
{
    public class ListLecturerDTO
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<LecturerProjectDTO>? LecturerProjectDTOs { get; set; }
    }
}
