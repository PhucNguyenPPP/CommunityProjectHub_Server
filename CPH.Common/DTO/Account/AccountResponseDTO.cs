using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Account
{
    public class AccountResponseDTO
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = null!;

        public string AccountName { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string? AvatarLink { get; set; }

        public string Phone { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;

        public bool Status { get; set; }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? AssociateName { get; set; }
    }
}
