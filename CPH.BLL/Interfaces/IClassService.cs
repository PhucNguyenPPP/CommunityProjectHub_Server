using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IClassService
    {
        Task<bool> CheckClassIdExist(Guid classId);
    }
}
