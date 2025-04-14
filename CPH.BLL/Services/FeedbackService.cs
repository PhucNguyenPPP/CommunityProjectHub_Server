using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> CreateFeedback(Guid accountId, Guid projectId, List<Guid> answerId, string ? feedbackContent)
        {
            var account = await _unitOfWork.Account.GetByCondition(c => c.AccountId == accountId);
            if(account == null)
            {
                return new ResponseDTO("Không tìm thấy tài khoản trùng khớp", 400, false);
            }

            var project = await _unitOfWork.Project.GetByCondition(c => c.ProjectId == projectId);
            if(project == null)
            {
                return new ResponseDTO("Không tìm thấy dự án trùng khớp", 400, false);
            }

            var questionList = _unitOfWork.Question.GetAll().ToList();
            if (answerId.Count != questionList.Count)
            {
                return new ResponseDTO("Bạn phải trả lời tất cả các câu hỏi.",400, false);
            }

            var trainee = await _unitOfWork.Trainee.GetByCondition(c => c.AccountId == accountId && c.Class.ProjectId == projectId);
            if(trainee == null)
            {
                return new ResponseDTO("Không tìm thấy học viên trùng khớp", 400, false);
            }
            var selectedQuestionIds = new List<Guid>();

            foreach (var id in answerId)
            {
                var answer = await _unitOfWork.Answer.GetByCondition(a => a.AnswerId == id);
                if (answer == null)
                {
                    return new ResponseDTO("Câu trả lời không hợp lệ", 400, false);
                }

                if (selectedQuestionIds.Contains(answer.QuestionId))
                {
                    return new ResponseDTO($"Chỉ được chọn một đáp án cho mỗi câu hỏi. Trùng câu hỏi: {answer.Question.QuestionContent}", 400, false);
                }
                selectedQuestionIds.Add(answer.QuestionId);

                var traineeAnswer = new TraineeAnswer
                {
                    TraineeAnswerId = Guid.NewGuid(),
                    AnswerId = answer.AnswerId,
                    TraineeId = trainee.TraineeId
                };
                await _unitOfWork.TraineeAnswer.AddAsync(traineeAnswer);
            }
            if (!feedbackContent.IsNullOrEmpty())
            {
                trainee.FeedbackContent = feedbackContent;
                _unitOfWork.Trainee.Update(trainee);
            }
           

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Lưu câu trả lời thành công", 201, true);
            }
            return new ResponseDTO("Lưu câu trả lời không thành công", 400, false);
        }
    }
}
