using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Registration
{
    public class RegistrationsOfProjectDTO
    {
        public Guid RegistrationId { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string Description { get; set; } = null!;

        public Guid ClassId { get; set; }

        public Guid AccountId { get; set; }
        public string ClassCode { get; set; } = null!;
        public Guid ProjectId { get; set; }

        public string Title { get; set; } = null!;
        public string AccountCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? DeniedReason { get; set; } = null!;

    }
}
