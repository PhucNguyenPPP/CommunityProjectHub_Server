using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.Answer;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Project;
using CPH.Common.DTO.Question;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CPH.BLL.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public QuestionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetAllQuestion(string? searchValue)
        {
            var list = _unitOfWork.Question.GetAll().Include(c => c.Answers).ToList();

            if (!searchValue.IsNullOrEmpty())
            {
                list = list.Where(c => c.QuestionContent.Contains(searchValue)).ToList();
                if (!list.Any())
                {
                    return new ResponseDTO("Tìm kiếm thấy nội dung hợp lệ", 400, false);
                }
            }

            var questionDTO = _mapper.Map<List<GetAllQuestionDTO>>(list);

            return new ResponseDTO("Lấy thông tin dự án thành công", 200, true, questionDTO);

        }
        
        public async Task<ResponseDTO> CreateQuestion(string questionContent, List<string> answers)
        {
            if (questionContent.IsNullOrEmpty())
            {
                return new ResponseDTO("Vui lòng nhập nội dung câu hỏi", 400, false);
            }

            if (answers.IsNullOrEmpty())
            {
                return new ResponseDTO("Vui lòng nhập nội dung câu trả lời", 400, false);
            }

            var form = _unitOfWork.Form.GetAll().FirstOrDefault();
            if (form == null)
            {
                return new ResponseDTO("Vui lòng tạo mẫu phản hồi khóa học", 400, false);
            }

            var newQuestion = new Question
            {
                QuestionId = Guid.NewGuid(),
                QuestionContent = questionContent,
                FormId = form.FormId,
            };

            await _unitOfWork.Question.AddAsync(newQuestion);

            foreach (var answerContent in answers)
            {
                if (answerContent.IsNullOrEmpty())
                {
                    return new ResponseDTO("Một trong các câu trả lời bị bỏ trống.", 400, false);
                }

                var answer = new Answer
                {
                    AnswerId = Guid.NewGuid(),
                    AnswerContent = answerContent.Trim(),
                    QuestionId = newQuestion.QuestionId
                };

                await _unitOfWork.Answer.AddAsync(answer);
            }

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Tạo câu hỏi thành công", 201, true);
            }
            return new ResponseDTO("Tạo câu hỏi không thành công", 400, false);
        }

        public async Task<ResponseDTO> DeleteQuestion(Guid questionId)
        {
            var question = await _unitOfWork.Question.GetByCondition(c => c.QuestionId == questionId);
            if (question == null)
            {
                return new ResponseDTO("Câu hỏi không tồn tại", 400, false);
            }

            var answer = _unitOfWork.Answer.GetAllByCondition(c => c.QuestionId == questionId);
            if(answer != null)
            {
                foreach (var answerContent in answer)
                {
                    _unitOfWork.Answer.Delete(answerContent);
                }
            }
            _unitOfWork.Question.Delete(question);

            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Xóa câu hỏi thành công", 201, true);
            }
            return new ResponseDTO("Xóa câu hỏi không thành công", 400, false);
        }

        public async Task<ResponseDTO> UpdateQuestion(Guid questionId, string questionContent, List<UpdateAnswerDTO> answers)
        {
            var question = await _unitOfWork.Question.GetByCondition(c => c.QuestionId == questionId);
            if(question == null)
            {
                return new ResponseDTO("Câu hỏi không tồn tại", 400, false);
            }

            question.QuestionContent = questionContent;
            _unitOfWork.Question.Update(question);

            var answerList = _unitOfWork.Answer.GetAllByCondition(c => c.QuestionId == questionId);
            if(answerList == null)
            {
                var result1 = await _unitOfWork.SaveChangeAsync();
                if (result1)
                {
                    return new ResponseDTO("Cập nhật câu hỏi thành công", 201, true);
                }
                return new ResponseDTO("Cập nhật câu hỏi không thành công", 400, false);
            }
            else
            {
                if (answerList.Count() != answers.Count())
                {
                    return new ResponseDTO($"Vui lòng nhập đủ số lượng câu trả lời: {answerList.Count()}", 400, false);
                }
                foreach (var oldAnswer in answerList)
                {
                    var updatedAnswer = answers.FirstOrDefault(a => a.AnswerId == oldAnswer.AnswerId);
                    if (updatedAnswer != null)
                    {
                        if (updatedAnswer.AnswerContent.IsNullOrEmpty())
                        {
                            return new ResponseDTO("Không được để trống nội dung câu trả lời", 400, false);
                        }
                        oldAnswer.AnswerContent = updatedAnswer.AnswerContent;
                        _unitOfWork.Answer.Update(oldAnswer);
                    }
                    else
                    {
                        return new ResponseDTO($"Không tìm thấy câu trả lời với ID: {oldAnswer.AnswerId}", 400, false);
                    }
                }
            }
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Cập nhật câu hỏi thành công", 201, true);
            }
            return new ResponseDTO("Cập nhật câu hỏi không thành công", 400, false);
        }
    }
}
