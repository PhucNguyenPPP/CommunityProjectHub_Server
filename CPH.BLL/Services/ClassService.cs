using CPH.BLL.Interfaces;
using CPH.Common.DTO.Message;
using CPH.DAL.UnitOfWork;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CheckClassIdExist(Guid classId)
        {
            var objClass = await _unitOfWork.Class.GetByCondition(c => c.ClassId == classId);
            if (objClass == null)
            {
                return false;
            }
            return true;
        }
    }
}
