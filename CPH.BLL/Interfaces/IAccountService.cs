using CPH.Common.DTO.Account;
using CPH.Common.DTO.General;
using CPH.DAL.Entities;
using Microsoft.AspNetCore.Http;
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
        byte[] GenerateSalt();
        byte[] GenerateHashedPassword(string password, byte[] saltBytes);
        Task<ResponseDTO> ImportAccountFromExcel(IFormFile file);
        List<string> CheckValidationImportAccountFromExcel(List<ImportAccountDTO> listAccount);
        List<string> CheckDuplicatedInDbImportAccountFromExcel(List<ImportAccountDTO> listAccount);
        Task<ResponseDTO> ImportTraineeFromExcel(IFormFile file);
        List<string> CheckValidationImportTraineeFromExcel(List<ImportTraineeDTO> listAccount);
        Task<List<string>> CheckInfoTraineeInDb(List<ImportTraineeDTO> listTrainee);
        bool CheckAccountIdExist(Guid accountId);
    }
}
