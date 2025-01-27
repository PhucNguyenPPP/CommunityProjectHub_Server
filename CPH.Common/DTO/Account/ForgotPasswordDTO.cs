using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Account
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression("^(?=.*[!@#$%^&*(),.?\":{}|<>]).+$",
           ErrorMessage = "Mật khẩu phải có ít nhất 1 ký tự đặc biệt")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu không giống nhau")]
        public string ConfirmedPassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; } = null!;
    }
}
