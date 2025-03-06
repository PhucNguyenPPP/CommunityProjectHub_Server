using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.Account;

namespace CPH.Common.DTO.ProjectLogging
{
    public class GetAllProjectLoggingDTO
    {
        public Guid ProjectNoteId { get; set; }

        public DateTime ActionDate { get; set; }

        public string ActionContent { get; set; } = null!;

        public string? NoteContent { get; set; }

        public Guid AccountId { get; set; }

        public Guid ProjectId { get; set; }

        public AccountResponseDTO Account { get; set; }
    }
}
