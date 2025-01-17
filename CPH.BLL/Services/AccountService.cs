using CPH.BLL.Interfaces;
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

        public List<Account> GetAllAccounts()
        {
            return _unitOfWork.Account.GetAll().ToList();
        }
    }
}
