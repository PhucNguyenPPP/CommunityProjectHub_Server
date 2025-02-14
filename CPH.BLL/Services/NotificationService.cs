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
        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            await _unitOfWork.Notification.AddAsync(notification);
        }

        public async Task<ResponseDTO> GetNotifications(Guid accountId)
        {
            var listNotification = _unitOfWork.Notification.GetAllByCondition(c => c.AccountId == accountId).ToList();
            if(listNotification.Count == 0)
            {
                return new ResponseDTO("Bạn chưa có bất kì thông báo nào", 404, false);
            }
            var mapList = _mapper.Map<List<NotificationResponseDTO>>(listNotification);
            return new ResponseDTO("Lấy thông báo thành công", 200, true, mapList);
        }
    }
}
