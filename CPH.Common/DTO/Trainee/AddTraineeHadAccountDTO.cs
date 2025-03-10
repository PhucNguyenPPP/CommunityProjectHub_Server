using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Trainee
{
    public class AddTraineeHadAccountDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của tài khoản học viên cần thêm vào lớp")]
        public Guid AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Id của lớp cần thêm học viên vào")]
        public Guid ClassId { get; set; }
    }
}
