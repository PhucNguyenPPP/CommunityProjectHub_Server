using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using CPH.Common.DTO.Lesson;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Common.DTO.Project
{
    public class NewProjectDTO
    {        
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
       /* [Required(ErrorMessage = "Vui lòng nhập số lượng học viên của mỗi nhóm")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng học viên/nhóm ít nhất bằng 1")]
        public int NumberTraineeEachGroup { get; set; }*/
        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu ứng tuyển của dự án")]
        public DateTime ApplicationStartDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày hết hạn ứng tuyển của dự án")]
        public DateTime ApplicationEndDate { get; set; }
        public Guid? ProjectManagerId { get; set; }
        /*
        [Required(ErrorMessage = "Vui lòng nhập file học viên của dự án")]
        
        public IFormFile Trainees { get; set; }
        */
        [Required(ErrorMessage = "Vui lòng nhập thông tin đối tác của dự án")]
        
        public Guid AssociateId { get; set; }
      [Required(ErrorMessage = "Vui lòng thời lượng tối thiểu của 1 bài học")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng tối thiểu của 1 bài học phải ít nhất là 1")]
        public int MinLessonTime { get; set; }
        [Required(ErrorMessage = "Vui lòng thời lượng tối đa của 1 bài học")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng tối đa của 1 bài học phải ít nhất là 1")]
        public int MaxLessonTime { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập danh sách bài học của dự án")]
        public List<string> LessonList { get; set; }
    }
}
