using CPH.BLL.Interfaces;
using CPH.Common.DTO.Material;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeFeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        public TraineeFeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [Authorize(Roles = "Trainee")]
        [HttpPost("new-feedback")]
        public async Task<IActionResult> CreateFeedback(Guid accountId, Guid projectId, List<Guid> answerId, string? feedbackContent)
        {
            var result = await _feedbackService.CreateFeedback(accountId, projectId, answerId, feedbackContent);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
