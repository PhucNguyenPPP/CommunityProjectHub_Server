using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.LessonClass;

namespace CPH.Common.DTO.Member
{
    public class MemberProjectDTO
    {
        public Guid MemberId { get; set; }

        public int GroupSupportNo { get; set; }

        public Guid ClassId { get; set; }

        public Guid AccountId { get; set; }

        public string ClassCode { get; set; } = null!;

        public AccountResponseDTO Account { get; set; } 

    }
}
