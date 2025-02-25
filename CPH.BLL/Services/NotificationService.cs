using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Notification;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly WebSocketHandler _webSocketHandler;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, WebSocketHandler webSocketHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _webSocketHandler = webSocketHandler;
        }

        public async Task CreateNotification(Guid accountId, string content)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                AccountId = accountId,
                MessageContent = content,
                IsRead = false
            };
            await _webSocketHandler.BroadcastNotificationAsync(notification);
            await _unitOfWork.Notification.AddAsync(notification);
        }

        public async Task<ResponseDTO> GetNotifications(Guid accountId)
        {
            var listNotification = _unitOfWork.Notification.GetAllByCondition(c => c.AccountId == accountId)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
            var mapList = _mapper.Map<List<NotificationResponseDTO>>(listNotification);
            return new ResponseDTO("Lấy thông báo thành công", 200, true, mapList);
        }
    }
}
