using CPH.DAL.Context;
using CPH.DAL.Entities;
using CPH.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.DAL.Repositories
{
    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(CphDbContext context) : base(context)
        {
        }
    }
}
