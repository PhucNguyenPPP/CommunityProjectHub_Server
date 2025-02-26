using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotification(Guid accountId, string content);
        Task<ResponseDTO> GetNotifications(Guid accountId);
        Task<ResponseDTO> UpdateIsReadNotification(UpdateNotificationRequestDTO model);
    }
}
