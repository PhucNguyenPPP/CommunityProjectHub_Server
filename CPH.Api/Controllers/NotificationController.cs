using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using CPH.Common.DTO.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotification(Guid accountId)
        {
            ResponseDTO responseDTO = await _notificationService.GetNotifications(accountId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 500 || responseDTO.StatusCode == 400)
                {
                    return BadRequest(responseDTO);
                }
            }

            return Ok(responseDTO);
        }

        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotification(UpdateNotificationRequestDTO model)
        {
            ResponseDTO responseDTO = await _notificationService.UpdateIsReadNotification(model);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 500 || responseDTO.StatusCode == 400)
                {
                    return BadRequest(responseDTO);
                }
            }

            return Ok(responseDTO);
        }
    }
}
