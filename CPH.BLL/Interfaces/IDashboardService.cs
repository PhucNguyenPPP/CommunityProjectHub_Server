using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.Common.DTO.General;

namespace CPH.BLL.Interfaces
{
    public interface IDashboardService
    {
        Task<ResponseDTO> GetAllNumberOfStudent();

        Task<ResponseDTO> GetAllNumberOfLecturer();

        Task<ResponseDTO> GetAllNumberOfTrainee(Guid accountId);

        Task<ResponseDTO> GetAllNumberOfProject(Guid accountId);

        Task<ResponseDTO> GetAllNumberOfProjectWithStatus(Guid accountId);

        ResponseDTO GetAllNumberOfUser();

        ResponseDTO GetAllNumberOfUserByRole();

        Task<ResponseDTO> GetProgressOfAllProject(Guid accountId);
    }
}
