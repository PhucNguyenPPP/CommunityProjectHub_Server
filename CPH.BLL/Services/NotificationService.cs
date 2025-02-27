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

        public async Task<ResponseDTO> UpdateIsReadNotification(UpdateNotificationRequestDTO model)
        {
            bool checkNotificationsExisted = true;
            foreach(var n in model.NotificationIds)
            {
                var notification = await _unitOfWork.Notification.GetByCondition(c => c.NotificationId == n);
                if(notification == null)
                {
                    checkNotificationsExisted = false;
                    break;
                }
            }

            if (!checkNotificationsExisted)
            {
                return new ResponseDTO("Thông báo không tồn tại", 400, false);
            }

            foreach (var n in model.NotificationIds)
            {
                var notification = await _unitOfWork.Notification.GetByCondition(c => c.NotificationId == n);
                notification!.IsRead = true;
                _unitOfWork.Notification.Update(notification);
            }

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Cập nhật thông báo thành công", 200, true);
            }
            return new ResponseDTO("Cập nhật thông báo thất bại", 400, false);
        }
    }
}
