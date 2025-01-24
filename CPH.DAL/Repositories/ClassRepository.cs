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
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(CphDbContext context) : base(context)
        {
        }
    }
}
