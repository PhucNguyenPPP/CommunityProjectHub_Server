using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Project;
using CPH.Common.DTO.Question;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ResponseDTO> GetAllQuestion()
        {
            var question = _unitOfWork.Question.GetAll().Include(c=> c.Answers);

            var questionDTO = _mapper.Map<List<GetAllQuestionDTO>>(question);

            return new ResponseDTO("Lấy thông tin dự án thành công", 200, true, questionDTO);

        } 
    }
}
