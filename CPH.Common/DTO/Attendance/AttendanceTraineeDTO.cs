using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Attendance
{
    public class AttendanceTraineeDTO
    {
        public string? AccountCode { get; set; }
        public string? TraineeFullName { get; set; }
        public int SlotNo { get; set; }
        public string? AttendanceStatus {  get; set; }
    }
}
