using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Trainee
{
    public class MoveTraineeClassDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của tài khoản học viên cần chuyển lớp")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lớp mà học viên muốn chuyển vào")]
        public Guid NewClassId { get; set; }
    }
}
