using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IClassService _classService;
        private readonly WebSocketHandler _websocketHandler;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, IAccountService accountService,
            IClassService classService, WebSocketHandler websocketHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountService = accountService;
            _classService = classService;
            _websocketHandler = websocketHandler;
        }

        public async Task<ResponseDTO> CreateMessage(MessageDTO newMessage)
        {
            var messageId = Guid.NewGuid();
            var createdDate = DateTime.Now;
            var mes = new Message()
            {
                MessageId = messageId,
                Content = newMessage.Content,
                CreatedDate = createdDate,
                AccountId = newMessage.AccountId,
                ClassId = newMessage.ClassId,
            };

            await _unitOfWork.Message.AddAsync(mes);
            bool ok = await _unitOfWork.SaveChangeAsync();
            if (ok)
            {
                var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == newMessage.AccountId);
                var wsMessage = new MessageResponseDTO()
                {
                    MessageId = messageId,
                    Content = newMessage.Content,
                    CreatedDate = createdDate,
                    SendAccountId = newMessage.AccountId,
                    ClassId = newMessage.ClassId,
                    SendAccountName = account.AccountName
                };

                await _websocketHandler.BroadcastMessageAsync(wsMessage);

                return new ResponseDTO("Gửi tin nhắn thành công", 200, true, null);
            }
            return new ResponseDTO("Gửi tin nhắn thất bại", 500, false, null);
        }

        public async Task<List<ClassChatDTO?>> GetAllClassChat(string? searchValue, Guid accountId)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if (account == null)
            {
                return new List<ClassChatDTO?>();
            }

            List<ClassChatDTO?> list = new List<ClassChatDTO?>();
            if (account.RoleId == (int)RoleEnum.Student)
            {
                List<Class>? listClass = new List<Class>();
                if (string.IsNullOrEmpty(searchValue))
                {
                    listClass = _unitOfWork.Member
                    .GetAllByCondition(c => c.AccountId == accountId)
                    .Include(c => c.Class)
                    .ThenInclude(c => c.Project)
                    .Select(c => c.Class)
                    .ToList();
                }
                else
                {
                    listClass = _unitOfWork.Member
                  .GetAllByCondition(c => c.AccountId == accountId)
                  .Include(c => c.Class)
                  .ThenInclude(c => c.Project)
                  .Select(c => c.Class)
                  .Where(c => c.ClassCode.ToLower().Contains(searchValue.ToLower()) || c.Project.Title.ToLower().Contains(searchValue.ToLower()))
                  .ToList();
                }

                foreach (var i in listClass)
                {
                    var lastMessageOfClass = _unitOfWork.Message
                        .GetAllByCondition(c => c.ClassId == i.ClassId)
                        .Include(c => c.Account)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefault();

                    var item = new ClassChatDTO()
                    {
                        ClassId = i.ClassId,
                        ClassCode = i.ClassCode,
                        Content = lastMessageOfClass != null ? lastMessageOfClass.Content : null,
                        ContentTimestamp = lastMessageOfClass != null ? lastMessageOfClass.CreatedDate : null,
                        ContentSender = lastMessageOfClass != null ? lastMessageOfClass.Account.AccountName : null,
                        ProjectId = i.Project.ProjectId,
                        ProjectTitle = i.Project.Title,
                    };
                    list.Add(item);
                }
            }

            if (account.RoleId == (int)RoleEnum.Lecturer)
            {

                List<Class>? listClass = new List<Class>();
                if (string.IsNullOrEmpty(searchValue))
                {
                    listClass = _unitOfWork.Class
                        .GetAllByCondition(c => c.LecturerId == accountId)
                        .Include(c => c.Project)
                        .ToList();
                }
                else
                {
                    listClass = _unitOfWork.Class
                       .GetAllByCondition(c => c.LecturerId == accountId)
                       .Include(c => c.Project)
                       .Where(c => c.ClassCode.ToLower().Contains(searchValue.ToLower()) || c.Project.Title.ToLower().Contains(searchValue.ToLower()))
                       .ToList();
                }


                foreach (var i in listClass)
                {
                    var lastMessageOfClass = _unitOfWork.Message
                        .GetAllByCondition(c => c.ClassId == i.ClassId)
                        .Include(c => c.Account)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefault();

                    var item = new ClassChatDTO()
                    {
                        ClassId = i.ClassId,
                        ClassCode = i.ClassCode,
                        Content = lastMessageOfClass != null ? lastMessageOfClass.Content : null,
                        ContentTimestamp = lastMessageOfClass != null ? lastMessageOfClass.CreatedDate : null,
                        ContentSender = lastMessageOfClass != null ? lastMessageOfClass.Account.AccountName : null,
                        ProjectId = i.Project.ProjectId,
                        ProjectTitle = i.Project.Title,
                    };
                    list.Add(item);
                }
            }

            if (account.RoleId == (int)RoleEnum.Trainee)
            {
                List<Class>? listClass = new List<Class>();
                if (string.IsNullOrEmpty(searchValue))
                {
                    listClass = _unitOfWork.Trainee
                        .GetAllByCondition(c => c.AccountId == accountId)
                        .Include(c => c.Class)
                        .ThenInclude(c => c.Project)
                        .Select(c => c.Class)
                        .ToList();
                }
                else
                {
                    listClass = _unitOfWork.Trainee
                        .GetAllByCondition(c => c.AccountId == accountId)
                        .Include(c => c.Class)
                        .ThenInclude(c => c.Project)
                        .Select(c => c.Class)
                        .Where(c => c.ClassCode.ToLower().Contains(searchValue.ToLower()) || c.Project.Title.ToLower().Contains(searchValue.ToLower()))
                        .ToList();
                }

                foreach (var i in listClass)
                {
                    var lastMessageOfClass = _unitOfWork.Message
                        .GetAllByCondition(c => c.ClassId == i.ClassId)
                        .Include(c => c.Account)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefault();

                    var item = new ClassChatDTO()
                    {
                        ClassId = i.ClassId,
                        ClassCode = i.ClassCode,
                        Content = lastMessageOfClass != null ? lastMessageOfClass.Content : null,
                        ContentTimestamp = lastMessageOfClass != null ? lastMessageOfClass.CreatedDate : null,
                        ContentSender = lastMessageOfClass != null ? lastMessageOfClass.Account.AccountName : null,
                        ProjectId = i.Project.ProjectId,
                        ProjectTitle = i.Project.Title,

                    };
                    list.Add(item);
                }
            }

            return list.OrderByDescending(c => c?.ContentTimestamp).ToList();
        }

        public async Task<ResponseDTO> GetMessages(Guid accountId, Guid classId)
        {
            var checkAccountId = _accountService.CheckAccountIdExist(accountId);
            if (!checkAccountId)
            {
                return new ResponseDTO("AccountId không tồn tại", 400, false);
            }

            var checkClassId = await _classService.CheckClassIdExist(classId);
            if (!checkClassId)
            {
                return new ResponseDTO("ClassId không tồn tại", 400, false);
            }

            var list = _unitOfWork.Message
              .GetAllByCondition(m => m.ClassId.Equals(classId))
              .Include(c => c.Account)
              .OrderBy(c => c.CreatedDate)
              .ToList();
            var listDTO = _mapper.Map<List<MessageResponseDTO>>(list);

            var classObj = _unitOfWork.Class
                .GetAllByCondition(c => c.ClassId.Equals(classId))
                .Include(c => c.Project)
                .FirstOrDefault();

            var result = new GetMessageResponseDTO()
            {
                ClassCode = classObj!.ClassCode,
                ProjectTitle = classObj!.Project.Title,
                MessageResponseDTOs = listDTO
            };

            return new ResponseDTO("Hiển thị tin nhắn thành công", 200, true, result);
        }
    }
}
