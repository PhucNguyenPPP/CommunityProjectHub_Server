using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.Lecturer;
using CPH.DAL.UnitOfWork;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LecturerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public List<LecturerResponseDTO> SearchLecturer(string searchValue)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<LecturerResponseDTO>();
            }

            var searchedList = _unitOfWork.Account.GetAllByCondition(c => c.AccountCode.ToLower().Contains(searchValue.ToLower())
            || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
            || c.Phone.ToLower().Contains(searchValue.ToLower()));

            var mappedSearchedList = _mapper.Map<List<LecturerResponseDTO>>(searchedList);
            return mappedSearchedList;
        }
    }
}
