using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IMessageService
    {
        Task<ResponseDTO> CreateMessage(MessageDTO newMessage);
        Task<ResponseDTO> GetMessages(Guid accountId, Guid classId);
        Task<List<ClassChatDTO?>> GetAllClassChat(string? searchValue, Guid userId);
    }
}
