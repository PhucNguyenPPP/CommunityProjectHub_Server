using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Auth
{
    public class LocalAccountDTO
    {
        public Guid AccountId { get; set; }

        public string AccountName { get; set; } = null!;

        public string AccountCode { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string AvatarLink { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;
        public bool Status { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? AssociateName { get; set; }
    }
}
