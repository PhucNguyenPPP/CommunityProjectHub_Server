using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Registration
{
    public class SendRegistrationDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của tài khoản đăng ký vào lớp")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lớp cần đăng ký")]
        public Guid ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả về người đăng ký")]
        public string Description { get; set; } = null!;
    }
}
