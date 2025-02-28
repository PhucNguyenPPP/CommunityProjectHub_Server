using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class DevideGroupOfClassDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập lớp muốn chia nhóm")]
        public Guid ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng nhóm")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhóm lớp có ít nhất bằng 1")]
        public int? NumberGroup { get; set; }
    }
}
