using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Message;
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
            var mes = new Message()
            {
                MessageId = Guid.NewGuid(),
                Content = newMessage.Content,
                CreatedDate = DateTime.Now,
                AccountId = newMessage.AccountId,
                ClassId = newMessage.ClassId,
            };

            await _websocketHandler.BroadcastMessageAsync(mes);
            
            await _unitOfWork.Message.AddAsync(mes);
            bool ok = await _unitOfWork.SaveChangeAsync();
            if (ok)
            {
                return new ResponseDTO("Gửi tin nhắn thành công", 200, true, null);
            }
            return new ResponseDTO("Gửi tin nhắn thất bại", 500, false, null);
        }

        public List<ClassChatDTO?> GetAllClassChat(Guid accountId)
        {
            var checkAccountId = _accountService.CheckAccountIdExist(accountId);
            if(!checkAccountId)
            {
                return new List<ClassChatDTO?>();
            }

            var listMessage = _unitOfWork.Message
                .GetAllByCondition(m => m.AccountId == accountId)
                .Include(c => c.Account)
                .Include(c => c.Class)
                .ThenInclude(c => c.Project)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            List<ClassChatDTO?> list = new List<ClassChatDTO?>();
            if (listMessage.Count > 0)
            {
                foreach (var i in listMessage)
                {
                    var item = new ClassChatDTO()
                    {
                        ClassId = i.ClassId,
                        ClassCode = i.Class.ClassCode,
                        Content = i.Content,
                        ProjectId = i.Class.ProjectId,
                        ProjectTitle = i.Class.Project.Title,
                    };
                    list.Add(item);
                }
            }
            return list;
        }

        public async Task<ResponseDTO> GetMessages(Guid accountId, Guid classId)
        {
            var checkAccountId = _accountService.CheckAccountIdExist(accountId);
            if (!checkAccountId)
            {
                return new ResponseDTO("AccountId không tồn tại", 400, false);
            }

            var checkClassId = await _classService.CheckClassIdExist(classId);
            if(!checkClassId)
            {
                return new ResponseDTO("ClassId không tồn tại", 400, false);
            }

            var list = _unitOfWork.Message
              .GetAllByCondition(m => m.ClassId.Equals(classId))
              .OrderBy(c => c.CreatedDate)
              .ToList();
            var listDTO = _mapper.Map<List<MessageResponseDTO>>(list);
            if (!listDTO.Any())
            {
                return new ResponseDTO("Không có tin nhắn", 404, false, listDTO);
            }
            return new ResponseDTO("Hiển thị tin nhắn thành công", 200, true, listDTO);
        }
    }
}
