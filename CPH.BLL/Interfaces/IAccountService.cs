using CPH.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IAccountService
    {
        List<Account> GetAllAccounts();
        bool CheckAccountNameExist(string accountName);
        bool CheckEmailExist(string email);
        bool CheckPhoneExist(string phone);
        bool CheckAccountCodeExist(string accountCode);
        string GenerateAccountCode(int role);
    }
}
