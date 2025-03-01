using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Message
{
    public class GetMessageResponseDTO
    {
        public string ClassCode { get; set; } = null!;
        public string ProjectTitle { get; set; } = null!;
        public List<MessageResponseDTO> MessageResponseDTOs { get; set; } = new List<MessageResponseDTO>();
    }
}
