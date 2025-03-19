using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Trainee
{
    public class MovingTraineeToAnotherGroupInClass
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của tài khoản học viên cần chuyển nhóm")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lớp của học viên")]
        public Guid ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nhóm mà học viên muốn chuyển vào")]
        [Range(1, int.MaxValue, ErrorMessage = "Nhóm cần chuyển vào có số thứ tự nhỏ nhất là 1")]
        public int NewGroupNo { get; set; }

    }
}
