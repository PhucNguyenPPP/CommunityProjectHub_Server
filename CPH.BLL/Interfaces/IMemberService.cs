﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Member;

namespace CPH.BLL.Interfaces
{
    public interface IMemberService
    {
        Task<ResponseDTO> GetAllMemberOfProject(Guid projectId, string? searchValue, int? pageNumber, int? rowsPerPage);
        Task<ResponseDTO> RemoveMemberFromProject(Guid memberId);
        List<MemberResponseDTO> SearchStudentForAssigningToClass(string? searchValue);
    }
}
