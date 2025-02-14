using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Class;
using CPH.Common.DTO.Paging;

namespace CPH.Common.DTO.Material
{
    public class ListMaterialDTO
    {
        public int? CurrentPage { get; set; }
        public int? RowsPerPages { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PagedList<GetAllMaterialDTO>? GetAllMaterialDTOs { get; set; }
    }
}
