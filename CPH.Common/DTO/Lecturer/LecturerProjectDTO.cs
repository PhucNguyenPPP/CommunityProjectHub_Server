using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Account;

namespace CPH.Common.DTO.Lecturer
{
    public class LecturerProjectDTO
    {
        public string ClassCode { get; set; } = null!;
        public AccountResponseDTO Account { get; set; }
    }
}
