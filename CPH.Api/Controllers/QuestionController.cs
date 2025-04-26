using System.ComponentModel.DataAnnotations;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.Answer;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Material;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [Authorize(Roles = "Business Relation")]
        [HttpGet("all-question-of-project")]
        public async Task<IActionResult> GetAllQuestion(string? searchValue)
        {
            ResponseDTO responseDTO = await _questionService.GetAllQuestion(searchValue);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 400)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500)
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Business Relation")]
        [HttpPost("new-question")]
        public async Task<IActionResult> CreateQuestion([Required]string questionContent, [Required] List<string> answers)
        {
            var result = await _questionService.CreateQuestion(questionContent, answers);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Business Relation")]
        [HttpDelete("question-of-project")]
        public async Task<IActionResult> DeleteQuestion([Required] Guid questionId)
        {
            ResponseDTO responseDTO = await _questionService.DeleteQuestion(questionId);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 400)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500)
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }

        [Authorize(Roles = "Business Relation")]
        [HttpPut("question-of-project")]
        public async Task<IActionResult> UpdateQuestion([Required] Guid questionId, [Required] string questionContent, List<string> answers)
        {
            ResponseDTO responseDTO = await _questionService.UpdateQuestion(questionId, questionContent, answers);
            if (responseDTO.IsSuccess == false)
            {
                if (responseDTO.StatusCode == 400)
                {
                    return NotFound(responseDTO);
                }
                if (responseDTO.StatusCode == 500)
                {
                    return BadRequest(responseDTO);
                }
            }
            return Ok(responseDTO);
        }
    }
}
