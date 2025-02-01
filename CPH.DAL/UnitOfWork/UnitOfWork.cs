using CPH.DAL.Context;
using CPH.DAL.Interfaces;
using CPH.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CphDbContext _context;
        public UnitOfWork()
        {
            _context = new CphDbContext();
            Account = new AccountRepository(_context);
            RefreshToken = new RefreshTokenRepository(_context);
            Message = new MessageRepository(_context);
            Notification = new NotificationRepository(_context);
            Class = new ClassRepository(_context);
            Project = new ProjectRepository(_context);
        }


        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<bool> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public IAccountRepository Account { get; private set; }
        public IRefreshTokenRepository RefreshToken { get; private set; }
        public IMessageRepository Message { get; private set; }
        public INotificationRepository Notification { get; private set; }
        public IClassRepository Class { get; private set; }
        public IProjectRepository Project { get; private set; }
    }
}
