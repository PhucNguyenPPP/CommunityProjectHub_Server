using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Project;
using CPH.Common.DTO.Registration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Utilities;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        [HttpPost("new-registration")]
        public async Task<IActionResult> SendRegistration([FromBody] SendRegistrationDTO registrationDTO)
        {

            ResponseDTO responseDTO = await _registrationService.SendRegistration(registrationDTO);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }
        [HttpPut("canceled-registration")]
        public async Task<IActionResult> CancelRegistration([FromBody] Guid cancelRegistrationId)
        {

            ResponseDTO responseDTO = await _registrationService.CancelRegistration(cancelRegistrationId);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }
        [HttpPut("registration")]
        public async Task<IActionResult> AnswerRegistration([FromBody] AnswerRegistrationDTO answerRegistrationDTO)
        {

            ResponseDTO responseDTO = await _registrationService.AnswerRegistration(answerRegistrationDTO);
            if (responseDTO.IsSuccess == false)
            {

                return BadRequest(responseDTO);

            }
            return Ok(responseDTO);
        }
    }
}
