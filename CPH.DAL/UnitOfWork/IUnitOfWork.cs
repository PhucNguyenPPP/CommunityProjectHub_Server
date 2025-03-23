using CPH.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.DAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        public void Dispose();
        public Task<bool> SaveChangeAsync();
        IAccountRepository Account { get; }
        IRefreshTokenRepository RefreshToken { get; }
        IMessageRepository Message { get; }
        INotificationRepository Notification { get; }
        IClassRepository Class { get; }
        IProjectRepository Project { get; }
        ITraineeRepository Trainee { get; }
        ILessonRepository Lesson { get; }
        ILessonClassRepository LessonClass { get; }
        IMaterialRepository Material { get; }
        IMemberRepository Member { get; }
        IRegistrationRepository Registration { get; }
        IProjectLoggingRepository ProjectLogging { get; }
        IAssociateRepository Associate { get; }
    }
}
