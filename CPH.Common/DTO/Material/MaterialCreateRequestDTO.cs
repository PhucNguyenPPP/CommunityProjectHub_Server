using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Material
{
    public class MaterialCreateRequestDTO
    {
        public IFormFile File { get; set; } = null!;
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public Guid UpdatedBy { get; set; }
    }
}
