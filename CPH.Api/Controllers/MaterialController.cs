using System.Buffers;
using CPH.BLL.Interfaces;
using CPH.BLL.Services;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Material;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Department Head")]
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

        [Authorize(Roles = "Student,Lecturer,Trainee,Department Head,Associate,Business Relation")]
        [HttpGet("all-material-of-project")]
        public async Task<IActionResult> GetAllMaterialProject([FromQuery] Guid projectId,
                                            [FromQuery] string? searchValue,
                                            [FromQuery] int? pageNumber,
                                            [FromQuery] int? rowsPerPage)
        {
            ResponseDTO responseDTO = await _materialService.GetAllMaterialProject(projectId, searchValue, pageNumber, rowsPerPage);
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

        [Authorize(Roles = "Department Head")]
        [HttpDelete("material-of-project")]
        public async Task<IActionResult> DeleteMaterial([FromQuery] Guid materialId)
        {
            ResponseDTO responseDTO = await _materialService.DeleteMaterial(materialId);
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

        [Authorize(Roles = "Department Head")]
        [HttpPut("material-of-project")]
        public async Task<IActionResult> UpdateMaterial([FromForm] MaterialUpdateDTO material)
        {
            ResponseDTO responseDTO = await _materialService.UpdateMaterial(material);
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
