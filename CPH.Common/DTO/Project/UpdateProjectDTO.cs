using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CPH.Common.DTO.Project
{
    public class UpdateProjectDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Id của dự án")]
        public Guid ProjectId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên của dự án")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Vui lòng nhập mô tả của dự án")]
        public string Description { get; set; } = null!;
        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu của dự án")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc của dự án")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa điểm của dự án")]
        public string Address { get; set; } = null!; //khong duoc nhieu hon so luong hoc vien
        /*
        [Required(ErrorMessage = "Vui lòng nhập số lượng học viên của mỗi nhóm")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng học viên/nhóm ít nhất bằng 1")]
        public int NumberTraineeEachGroup { get; set; }
        */
        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu ứng tuyển của dự án")]
        public DateTime ApplicationStartDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày hết hạn ứng tuyển của dự án")]
        public DateTime ApplicationEndDate { get; set; }

    }
}
