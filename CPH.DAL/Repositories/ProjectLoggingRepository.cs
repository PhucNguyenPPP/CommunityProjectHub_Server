﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPH.DAL.Context;
using CPH.DAL.Entities;
using CPH.DAL.Interfaces;

namespace CPH.DAL.Repositories
{
    public class ProjectLoggingRepository : GenericRepository<ProjectLogging>, IProjectLoggingRepository
    {
        public ProjectLoggingRepository(CphDbContext context) : base(context)
        {
        }
    }
}
