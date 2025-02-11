using CPH.BLL.Interfaces;
using CPH.Common.DTO.Material;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CPH.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpPost("new-material")]
        public async Task<IActionResult> CreateMaterial([FromForm] MaterialCreateRequestDTO model)
        {
            var result = await _materialService.CreateMaterial(model);
            if(result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
