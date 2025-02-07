using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Account
{
    public class AccountEmailDTO
    {
        public string Email { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
