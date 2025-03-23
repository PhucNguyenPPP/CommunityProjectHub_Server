using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Associate;
using CPH.Common.DTO.General;
using CPH.Common.DTO.Lecturer;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class AssociateService : IAssociateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public AssociateService(IUnitOfWork unitOfWork, IMapper mapper, IAccountService accountService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountService = accountService;
            _emailService = emailService;
        }

        public List<AssociateResponseDTO> SearchAssociateToAssignToProject(string? searchValue)
        {
            if (searchValue.IsNullOrEmpty())
            {
                return new List<AssociateResponseDTO>();
            }

            List<Account> searchedList = _unitOfWork.Account.GetAllByCondition(c => (c.AccountCode.ToLower().Contains(searchValue!.ToLower())
                || c.FullName.ToLower().Contains(searchValue.ToLower()) || c.Email.ToLower().Contains(searchValue.ToLower())
                || c.Phone.ToLower().Contains(searchValue.ToLower())) && c.RoleId == (int)RoleEnum.Associate)
                .Include(c => c.Associate)
                .ToList();

            var mappedSearchedList = _mapper.Map<List<AssociateResponseDTO>>(searchedList);
            return mappedSearchedList;
        }

        public async Task<bool> SignUpAssociate(SignUpAssociateRequestDTO model)
        {
            var account = _mapper.Map<Account>(model);

            var salt = _accountService.GenerateSalt();
            var generatedPassword = _accountService.GeneratePasswordString();
            var passwordHash = _accountService.GenerateHashedPassword(generatedPassword, salt);
            //var avatarLink = await _imageService.StoreImageAndGetLink(model.AvatarLink, FileNameFirebaseStorage.UserImage);

            var accountId = Guid.NewGuid();
            account.AccountId = accountId;
            account.AccountCode = _accountService.GenerateAccountCode((int)RoleEnum.Associate);
            account.Salt = salt;
            account.PasswordHash = passwordHash;
            account.Status = true;

            await _unitOfWork.Account.AddAsync(account);

            var associateAccount = new Associate
            {
                AccountId = accountId,
                AssociateName = model.AssociateName,
            };
            await _unitOfWork.Associate.AddAsync(associateAccount);

            await _emailService.SendAccountEmail(account.Email, account.AccountName, generatedPassword, "The Community Project Hub's account");
            return await _unitOfWork.SaveChangeAsync();
        }

        public ResponseDTO CheckValidationSignUpAssociate(SignUpAssociateRequestDTO model)
        {
            if (model.DateOfBirth >= DateTime.Now)
            {
                return new ResponseDTO("Ngày sinh phải nhỏ hơn ngày hiện tại", 400, false);
            }

            if (model.Gender != GenderConstant.Male
                && model.Gender != GenderConstant.Female)
            {
                return new ResponseDTO("Giới tính không hợp lệ", 400, false);
            }

            var checkAccountNameExist = _accountService.CheckAccountNameExist(model.AccountName);
            if (checkAccountNameExist)
            {
                return new ResponseDTO("Tên tài khoản đã tồn tại", 400, false);
            }

            var checkEmailExist = _accountService.CheckEmailExist(model.Email);
            if (checkEmailExist)
            {
                return new ResponseDTO("Email đã tồn tại", 400, false);
            }

            var checkPhoneExist = _accountService.CheckPhoneExist(model.Phone);
            if (checkPhoneExist)
            {
                return new ResponseDTO("Số điện thoại đã tồn tại", 400, false);
            }

            var checkAssociateNameExist = _accountService.CheckAssociateNameExist(model.AssociateName);
            if (checkAssociateNameExist)
            {
                return new ResponseDTO("Tên đối tác đã tồn tại", 400, false);

            }
            return new ResponseDTO("Kiểm tra thành công", 200, true);
        } 
    }
}
