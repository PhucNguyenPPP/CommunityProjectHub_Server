using CPH.Common.DTO.Associate;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IAssociateService
    {
        List<AssociateResponseDTO> SearchAssociateToAssignToProject(string? searchValue);
        Task<bool> SignUpAssociate(SignUpAssociateRequestDTO model);
        ResponseDTO CheckValidationSignUpAssociate(SignUpAssociateRequestDTO model);
    }
}
