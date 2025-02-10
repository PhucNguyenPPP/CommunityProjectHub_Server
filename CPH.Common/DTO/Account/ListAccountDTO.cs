using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Paging;
using CPH.Common.DTO.Project;

namespace CPH.Common.DTO.Account
{
    public class ListAccountDTO
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<AccountResponseDTO>? AccountResponseDTOs { get; set; }
    }
}
