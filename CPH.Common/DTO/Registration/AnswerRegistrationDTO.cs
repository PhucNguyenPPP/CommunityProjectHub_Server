using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Registration
{
    public class AnswerRegistrationDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của đơn đăng ký")]
        public Guid RegistrationId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập kết quả của đơn đăng ký")]
        public string Type { get; set; } = null!;
    }
}
