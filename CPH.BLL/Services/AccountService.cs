using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CheckEmailExist(string email)
        {
            var accountList = _unitOfWork.Account.GetAll();
            if (accountList.Any(c => c.Email == email))
            {
                return true;
            }
            return false;
        }

        public bool CheckAccountNameExist(string accountName)
        {
            var accountList = _unitOfWork.Account.GetAll();
            if (accountList.Any(c => c.AccountName == accountName))
            {
                return true;
            }
            return false;
        }

        public bool CheckPhoneExist(string phone)
        {
            var accountList = _unitOfWork.Account.GetAll();
            if (accountList.Any(c => c.Phone == phone))
            {
                return true;
            }
            return false;
        }

        public List<Account> GetAllAccounts()
        {
            return _unitOfWork.Account.GetAll().ToList();
        }

        public bool CheckAccountCodeExist(string code)
        {
            var accountList = _unitOfWork.Account.GetAll();
            if (accountList.Any(c => c.AccountCode == code))
            {
                return true;
            }
            return false;
        }


        public string GenerateAccountCode(int role)
        {
            Random random = new Random();
            string accountCode = "";
            bool checkAccountCodeExist = false;
            do
            {
                string digit = random.Next(100000, 999999).ToString();
                switch (role)
                {
                    case (int)RoleEnum.Student:
                        accountCode = PreCodeConstant.PreCodeStudent + digit;
                        break;
                    case (int)RoleEnum.Lecturer:
                        accountCode = PreCodeConstant.PreCodeLecturer + digit;
                        break;
                    case (int)RoleEnum.Trainee:
                        accountCode = PreCodeConstant.PreCodeTrainee + digit;
                        break;
                    case (int)RoleEnum.Associate:
                        accountCode = PreCodeConstant.PreCodeAssociate + digit;
                        break;
                    default:
                        return string.Empty;
                }

                checkAccountCodeExist = CheckAccountCodeExist(accountCode);
            } while (checkAccountCodeExist);

            return accountCode;

        }
    }
}
