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
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(CphDbContext context) : base(context)
        {
        }
    }
}
