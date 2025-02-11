using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Material
{
    public class GetAllMaterialDTO
    {
        public Guid MaterialId { get; set; }

        public string Title { get; set; } = null!;

        public string MaterialUrl { get; set; } = null!;

        public DateTime UploadedAt { get; set; }

        public Guid ProjectId { get; set; }
    }
}
