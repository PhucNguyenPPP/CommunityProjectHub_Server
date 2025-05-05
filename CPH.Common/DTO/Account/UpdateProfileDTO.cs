using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Account
{
    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập accountId")]
        public Guid AccountId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MinLength(8, ErrorMessage = "Họ tên phải có ít nhất 8 ký tự")]
        [RegularExpression("^[\\p{L}]+([\\s\\p{L}]+)*$",
            ErrorMessage = "Họ tên không hợp lệ")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression("^0\\d{9}$",
            ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giới tính")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public string DateOfBirth { get; set; } = null!;
    }
}
