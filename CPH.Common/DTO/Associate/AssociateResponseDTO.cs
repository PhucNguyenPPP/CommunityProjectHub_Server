using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Associate
{
    public class AssociateResponseDTO
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

        public string AssociateName { get; set; } = null!;
    }
}
