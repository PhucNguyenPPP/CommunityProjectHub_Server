using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Class
{
    public class RemoveUpdateClassDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản cần thay thế")]
        public Guid RemovedAccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lớp cần thay đổi nhân sự")]
        public Guid ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tài khoản của nhân sự thay thế")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vai trò cần thay đổi nhân sự của lớp")]
        [Range(1, 2, ErrorMessage = "Vui lòng nhập vai trò hợp lệ: Giảng viên, sinh viên hỗ trợ")]
        public int RoleId { get; set; }
    }
}
