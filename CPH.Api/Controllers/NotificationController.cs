using CPH.BLL.Interfaces;
using CPH.BLL.WebSocketHandler;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                if (responseDTO.StatusCode == 404)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500 || responseDTO.StatusCode == 400)
                {
                    return BadRequest(responseDTO);
                }
            }

            return Ok(responseDTO);
        }
    }
}
