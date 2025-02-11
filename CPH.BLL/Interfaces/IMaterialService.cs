using CPH.Common.DTO.General;
using CPH.Common.DTO.Material;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IMaterialService
    {
        Task<ResponseDTO> CreateMaterial(MaterialCreateRequestDTO model);
        Task<string> StoreFileAndGetLink(IFormFile file, string folderName);
    }
}
