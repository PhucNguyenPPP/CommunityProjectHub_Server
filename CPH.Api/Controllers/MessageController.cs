﻿using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly WebSocketHandler _webSocketHandler;
        public MessageController(IMessageService messageService, WebSocketHandler webSocketHandler)
        {
            _messageService = messageService;
            _webSocketHandler = webSocketHandler;
        }
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessage(Guid accountId, Guid classId)
        {
            ResponseDTO responseDTO = await _messageService.GetMessages(accountId, classId);
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
        [HttpPost("message")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageDTO messageDTO)
        {
            ResponseDTO responseDTO = await _messageService.CreateMessage(messageDTO);
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

        [HttpGet("chat-classes")]
        public IActionResult GetAllClassChat(Guid accountId)
        {
            var result = _messageService.GetAllClassChat(accountId);
            if (result.Count > 0)
            {
                return Ok(new ResponseDTO("Lấy toàn bộ đoạn chat thành công", 200, true, result));
            }
            else
            {
                return NotFound(new ResponseDTO("Không tìm thấy đoạn chat nào", 404, false, result));
            }
        }
    }
}
