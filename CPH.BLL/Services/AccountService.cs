using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.Constant;
using CPH.Common.DTO.Account;
using CPH.Common.DTO.Email;
using CPH.Common.DTO.General;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public byte[] GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(saltBytes);
            return saltBytes;
        }

        public byte[] GenerateHashedPassword(string password, byte[] saltBytes)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];

            for (int i = 0; i < passwordBytes.Length; i++)
            {
                passwordWithSaltBytes[i] = passwordBytes[i];
            }

            for (int i = 0; i < saltBytes.Length; i++)
            {
                passwordWithSaltBytes[passwordBytes.Length + i] = saltBytes[i];
            }

            var cryptoProvider = SHA512.Create();
            byte[] hashedBytes = cryptoProvider.ComputeHash(passwordWithSaltBytes);

            return hashedBytes;
        }

        public async Task<ResponseDTO> ImportAccountFromExcel(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var accounts = new List<ImportAccountDTO>();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var account = new ImportAccountDTO
                    {
                        AccountCode = worksheet.Cells[row, 2].Text,
                        AccountName = worksheet.Cells[row, 3].Text,
                        Password = worksheet.Cells[row, 4].Text,
                        FullName = worksheet.Cells[row, 5].Text,
                        Phone = worksheet.Cells[row, 6].Text,
                        Email = worksheet.Cells[row, 7].Text,
                        Address = worksheet.Cells[row, 8].Text,
                        DateOfBirth = worksheet.Cells[row, 9].Text,
                        Gender = worksheet.Cells[row, 10].Text,
                        RoleName = worksheet.Cells[row, 11].Text
                    };
                    accounts.Add(account);
                }
            }

            var resultCheckValidation = CheckValidationImportAccountFromExcel(accounts);
            if (resultCheckValidation.Count > 0)
            {
                return new ResponseDTO("File excel không hợp lệ", 400, false, resultCheckValidation);
            }

            var resultCheckDuplicatedInDb = CheckDuplicatedInDbImportAccountFromExcel(accounts);
            if (resultCheckDuplicatedInDb.Count > 0)
            {
                return new ResponseDTO("File excel không hợp lệ", 400, false, resultCheckDuplicatedInDb);
            }

            var mapList = _mapper.Map<List<Account>>(accounts);

            for (var i = 0; i < mapList.Count; i++)
            {
                mapList[i].AccountId = Guid.NewGuid();
                mapList[i].Status = true;
                mapList[i].Salt = GenerateSalt();
                mapList[i].PasswordHash = GenerateHashedPassword(accounts[i].Password, mapList[i].Salt);

                if (accounts[i].RoleName.ToLower().Equals("student"))
                {
                    mapList[i].RoleId = (int)RoleEnum.Student;
                }

                if (accounts[i].RoleName.ToLower().Equals("trainee"))
                {
                    mapList[i].RoleId = (int)RoleEnum.Trainee;
                }

                if (accounts[i].RoleName.ToLower().Equals("lecturer"))
                {
                    mapList[i].RoleId = (int)RoleEnum.Lecturer;
                }

                if (accounts[i].RoleName.ToLower().Equals("associate"))
                {
                    mapList[i].RoleId = (int)RoleEnum.Associate;
                }
            }

            await _unitOfWork.Account.AddRangeAsync(mapList);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result)
            {
                return new ResponseDTO("Import tài khoản thành công", 201, true);
            }

            return new ResponseDTO("Import tài khoản thất bại", 400, false);
        }

        public List<string> CheckValidationImportAccountFromExcel(List<ImportAccountDTO> listAccount)
        {
            var listResult = new List<string>();

            if (listAccount == null || listAccount.Count == 0)
            {
                listResult.Add("Danh sách tài khoản trống");
                return listResult;
            }

            var accountCodeSet = new Dictionary<string, List<int>>();
            var accountNameSet = new Dictionary<string, List<int>>();
            var emailSet = new Dictionary<string, List<int>>();
            var phoneSet = new Dictionary<string, List<int>>();
            var passwordSet = new Dictionary<string, List<int>>();

            for (int i = 0; i < listAccount.Count; i++)
            {
                var account = listAccount[i];
                int accountNumber = i + 1;

                if (string.IsNullOrEmpty(account.AccountCode))
                {
                    listResult.Add($"Mã số tài khoản của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!accountCodeSet.ContainsKey(account.AccountCode))
                    {
                        accountCodeSet[account.AccountCode] = new List<int>();
                    }
                    accountCodeSet[account.AccountCode].Add(accountNumber);

                    if (account.AccountCode.Length < 8)
                    {
                        listResult.Add($"Mã số tài khoản của tài khoản số {accountNumber} phải có ít nhất 8 ký tự");
                    }
                }

                if (string.IsNullOrEmpty(account.AccountName))
                {
                    listResult.Add($"Tên tài khoản của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!accountNameSet.ContainsKey(account.AccountName))
                    {
                        accountNameSet[account.AccountName] = new List<int>();
                    }
                    accountNameSet[account.AccountName].Add(accountNumber);

                    if (account.AccountName.Length < 5)
                    {
                        listResult.Add($"Tên tài khoản của tài khoản số {accountNumber} phải có ít nhất 5 ký tự");
                    }

                }

                if (string.IsNullOrEmpty(account.Password))
                {
                    listResult.Add($"Mật khẩu của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {

                    Regex regexPassword = new Regex("^(?=.*[!@#$%^&*(),.?\":{}|<>]).+$");
                    if (account.Password.Length < 8 || !regexPassword.IsMatch(account.Password))
                    {
                        listResult.Add($"Mật khẩu của tài khoản số {accountNumber} phải có ít nhất 8 ký tự và có ít nhất 1 ký tự đặc biệt");
                    }
                }

                if (string.IsNullOrEmpty(account.FullName))
                {
                    listResult.Add($"Họ và tên của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (account.FullName.Length < 8)
                    {
                        listResult.Add($"Họ và tên của tài khoản số {accountNumber} phải có ít nhất 8");
                    }
                }

                if (string.IsNullOrEmpty(account.Phone))
                {
                    listResult.Add($"Số điện thoại của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!phoneSet.ContainsKey(account.Phone))
                    {
                        phoneSet[account.Phone] = new List<int>();
                    }
                    phoneSet[account.Phone].Add(accountNumber);

                    Regex regexPhone = new Regex("^0\\d{9}$");
                    if (!regexPhone.IsMatch(account.Phone))
                    {
                        listResult.Add($"Số điện thoại của tài khoản số {accountNumber} không hợp lệ");
                    }
                }

                if (string.IsNullOrEmpty(account.Email))
                {
                    listResult.Add($"Email của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!emailSet.ContainsKey(account.Email))
                    {
                        emailSet[account.Email] = new List<int>();
                    }
                    emailSet[account.Email].Add(accountNumber);

                    Regex regexEmail = new Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
                    if (!regexEmail.IsMatch(account.Email))
                    {
                        listResult.Add($"Email của tài khoản số {accountNumber} không hợp lệ");
                    }
                }

                if (string.IsNullOrEmpty(account.Address))
                {
                    listResult.Add($"Địa chỉ của tài khoản số {accountNumber} không có dữ liệu");
                }

                if (string.IsNullOrEmpty(account.DateOfBirth))
                {
                    listResult.Add($"Ngày sinh của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    bool checkParseDob = DateTime.TryParse(account.DateOfBirth, out DateTime parseDob);
                    if (!checkParseDob || parseDob >= DateTime.Now)
                    {
                        listResult.Add($"Ngày sinh của tài khoản số {accountNumber} không hợp lệ");
                    }
                }

                if (string.IsNullOrEmpty(account.Gender))
                {
                    listResult.Add($"Giới tính của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!account.Gender.Equals("Nam") && !account.Gender.Equals("Nữ"))
                    {
                        listResult.Add($"Giới tính của tài khoản số {accountNumber} không hợp lệ");
                    }
                }

                if (string.IsNullOrEmpty(account.RoleName))
                {
                    listResult.Add($"Vai trò của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (!account.RoleName.ToLower().Equals("student") && !account.RoleName.ToLower().Equals("trainee")
                        && !account.RoleName.ToLower().Equals("associate") && !account.RoleName.ToLower().Equals("lecturer"))
                    {
                        listResult.Add($"Vai trò của tài khoản số {accountNumber} không hợp lệ");
                    }
                }
            }

            CheckDuplicates(accountCodeSet, "Mã số tài khoản", listResult);
            CheckDuplicates(accountNameSet, "Tên tài khoản", listResult);
            CheckDuplicates(emailSet, "Email", listResult);
            CheckDuplicates(phoneSet, "Số điện thoại", listResult);
            CheckDuplicates(passwordSet, "Mật khẩu", listResult);

            return listResult;
        }

        private void CheckDuplicates(Dictionary<string, List<int>> dictionary, string fieldName, List<string> listResult)
        {
            foreach (var item in dictionary)
            {
                if (item.Value.Count > 1)
                {
                    string duplicateInfo = string.Join(", ", item.Value);
                    listResult.Add($"{fieldName} '{item.Key}' bị trùng ở các tài khoản số: {duplicateInfo}");
                }
            }
        }

        public List<string> CheckDuplicatedInDbImportAccountFromExcel(List<ImportAccountDTO> listAccount)
        {
            var listResult = new List<string>();

            if (listAccount == null || listAccount.Count == 0)
            {
                listResult.Add("Danh sách tài khoản trống");
                return listResult;
            }

            for (int i = 0; i < listAccount.Count; i++)
            {
                var account = listAccount[i];
                int accountNumber = i + 1;
                var accountListDb = _unitOfWork.Account.GetAll();

                if (string.IsNullOrEmpty(account.AccountCode))
                {
                    listResult.Add($"Mã số tài khoản của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (accountListDb.Any(c => c.AccountCode == account.AccountCode))
                    {
                        listResult.Add($"Mã số tài khoản của tài khoản số {accountNumber} đã tồn tại");
                    }
                }

                if (string.IsNullOrEmpty(account.AccountName))
                {
                    listResult.Add($"Tên tài khoản của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (accountListDb.Any(c => c.AccountName == account.AccountName))
                    {
                        listResult.Add($"Tên tài khoản của tài khoản số {accountNumber} đã tồn tại");
                    }

                }

                if (string.IsNullOrEmpty(account.Phone))
                {
                    listResult.Add($"Số điện thoại của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (accountListDb.Any(c => c.Phone == account.Phone))
                    {
                        listResult.Add($"Số điện thoại của tài khoản số {accountNumber} đã tồn tại");
                    }

                }

                if (string.IsNullOrEmpty(account.Email))
                {
                    listResult.Add($"Email của tài khoản số {accountNumber} không có dữ liệu");
                }
                else
                {
                    if (accountListDb.Any(c => c.Email == account.Email))
                    {
                        listResult.Add($"Email của tài khoản số {accountNumber} đã tồn tại");
                    }
                }
            }
            return listResult;
        }

        public async Task<ResponseDTO> ImportTraineeFromExcel(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var allClasses = new Dictionary<string, List<ImportAccountDTO>>();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var accounts = new List<ImportTraineeDTO>();

            using (var package = new ExcelPackage(stream))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var account = new ImportTraineeDTO
                        {
                            AccountCode = worksheet.Cells[row, 2].Text,
                            AccountName = worksheet.Cells[row, 3].Text,
                            FullName = worksheet.Cells[row, 4].Text,
                            Phone = worksheet.Cells[row, 5].Text,
                            Email = worksheet.Cells[row, 6].Text,
                            Address = worksheet.Cells[row, 7].Text,
                            DateOfBirth = worksheet.Cells[row, 8].Text,
                            Gender = worksheet.Cells[row, 9].Text,
                            ClassCode = worksheet.Name,
                        };
                        accounts.Add(account);
                    }
                }
            }

            var checkValidation = CheckValidationImportTraineeFromExcel(accounts);

            if (checkValidation.Count > 0)
            {
                return new ResponseDTO("File excel học viên không hợp lệ", 400, false, checkValidation);
            }

            var checkInfoTraineeInDb = await CheckInfoTraineeInDb(accounts);
            if(checkInfoTraineeInDb.Count > 0)
            {
                return new ResponseDTO("File excel học viên không hợp lệ", 400, false, checkInfoTraineeInDb);
            }

            return new ResponseDTO("Import học viên thành công", 201, true, accounts);
        }

        public List<string> CheckValidationImportTraineeFromExcel(List<ImportTraineeDTO> listTrainee)
        {
            var listResult = new List<string>();

            if (listTrainee == null || listTrainee.Count == 0)
            {
                listResult.Add("Danh sách tài khoản trống");
                return listResult;
            }

            // Dictionary để kiểm tra mỗi học viên chỉ được tồn tại ở 1 lớp duy nhất
            var studentClassMap = new Dictionary<string, string>();

            // Dictionary để kiểm tra học viên trùng trong cùng 1 lớp
            var classTraineeMap = new Dictionary<string, HashSet<string>>();

            for (int i = 0; i < listTrainee.Count; i++)
            {
                var trainee = listTrainee[i];
                int traineeNumber = i + 1;

                // Kiểm tra dữ liệu trống
                if (string.IsNullOrEmpty(trainee.AccountCode))
                {
                    listResult.Add($"Mã số tài khoản của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.AccountName))
                {
                    listResult.Add($"Tên tài khoản của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.FullName))
                {
                    listResult.Add($"Họ và tên của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.Phone))
                {
                    listResult.Add($"Số điện thoại của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.Email))
                {
                    listResult.Add($"Email của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.Address))
                {
                    listResult.Add($"Địa chỉ của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.DateOfBirth))
                {
                    listResult.Add($"Ngày sinh của học viên số {traineeNumber} không có dữ liệu");
                }
                if (string.IsNullOrEmpty(trainee.Gender))
                {
                    listResult.Add($"Giới tính của học viên số {traineeNumber} không có dữ liệu");
                }

                // Kiểm tra mỗi học viên chỉ có thể thuộc 1 lớp duy nhất
                if (!string.IsNullOrEmpty(trainee.AccountCode) && !string.IsNullOrEmpty(trainee.ClassCode))
                {
                    if (studentClassMap.ContainsKey(trainee.AccountCode))
                    {
                        if (studentClassMap[trainee.AccountCode] != trainee.ClassCode)
                        {
                            listResult.Add($"Học viên {trainee.FullName} (Mã: {trainee.AccountCode}) đã tồn tại trong lớp {studentClassMap[trainee.AccountCode]}, không thể đăng ký vào lớp {trainee.ClassCode}");
                        }
                    }
                    else
                    {
                        studentClassMap[trainee.AccountCode] = trainee.ClassCode;
                    }

                    // Kiểm tra học viên trùng trong cùng 1 lớp
                    if (!classTraineeMap.ContainsKey(trainee.ClassCode))
                    {
                        classTraineeMap[trainee.ClassCode] = new HashSet<string>();
                    }

                    if (!classTraineeMap[trainee.ClassCode].Add(trainee.AccountCode))
                    {
                        listResult.Add($"Học viên {trainee.FullName} (Mã: {trainee.AccountCode}) đã tồn tại trong lớp {trainee.ClassCode}");
                    }
                }
            }

            return listResult;
        }

        public async Task<List<string>> CheckInfoTraineeInDb(List<ImportTraineeDTO> listTrainee)
        {
            var listResult = new List<string>();

            if (listTrainee == null || listTrainee.Count == 0)
            {
                listResult.Add("Danh sách tài khoản trống");
                return listResult;
            }

            for (int i = 0; i < listTrainee.Count; i++)
            {
                var trainee = listTrainee[i];
                int traineeNumber = i + 1;
                
                var traineeDb = await _unitOfWork.Account.GetByCondition(c => c.AccountCode == trainee.AccountCode);
                if(traineeDb == null)
                {
                    listResult.Add($"Học viên {trainee.FullName} (Mã: {trainee.AccountCode}) không tồn tại trong hệ thống");
                } else
                {
                    if (!traineeDb.AccountName.Equals(trainee.AccountName))
                    {
                        listResult.Add($"Tên tài khoản của học viên {trainee.FullName} (Mã: {trainee.AccountCode}) bị sai lệch");
                    }

                    if (!traineeDb.FullName.Equals(trainee.FullName))
                    {
                        listResult.Add($"Họ và tên của học viên {trainee.FullName} (Mã: {trainee.AccountCode}) bị sai lệch");
                    }

                    if (!traineeDb.Phone.Equals(trainee.Phone))
                    {
                        listResult.Add($"Số điện thoại của học viên {trainee.FullName} (Mã: {trainee.AccountCode}) bị sai lệch");
                    }

                    if (!traineeDb.Address.ToLower().Equals(trainee.Address.ToLower()))
                    {
                        listResult.Add($"Địa chỉ của học viên {trainee.FullName} (Mã: {trainee.AccountCode}) bị sai lệch");
                    }

                    if (!traineeDb.Gender.ToLower().Equals(trainee.Gender.ToLower()))
                    {
                        listResult.Add($"Giới tính của học viên {trainee.FullName} (Mã: {trainee.AccountCode}) bị sai lệch");
                    }
                }
            }

             return listResult;
        }

        public bool CheckAccountIdExist(Guid accountId)
        {
            var accountList = _unitOfWork.Account.GetAll();
            if (accountList.Any(c => c.AccountId == accountId))
            {
                return true;
            }
            return false;
        }

        public Task<Account?> GetAccountByEmail(string email)
        {
            return _unitOfWork.Account.GetByCondition(c => c.Email == email);
        }

        public async Task<bool> SetOtp(string email, OtpCodeDTO model)
        {
            var account = await GetAccountByEmail(email);
            if (account != null)
            {
                account.OtpCode = Int32.Parse(model.OTPCode);
                account.OtpExpiredTime = model.ExpiredTime;
                return await _unitOfWork.SaveChangeAsync();
            }
            return false;
        }

        public async Task<bool> VerifyingOtp(string email, string otp)
        {
            var account = await GetAccountByEmail(email);
            if (account != null)
            {
                if (account.OtpCode == Int32.Parse(otp) && account.OtpExpiredTime > DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ChangePassword(ForgotPasswordDTO model)
        {
            var account = await GetAccountByEmail(model.Email);
            if (account == null)
            {
                return false;
            }

            var salt = GenerateSalt();
            var passwordHash = GenerateHashedPassword(model.Password, salt);
            account.Salt = salt;
            account.PasswordHash = passwordHash;
            return await _unitOfWork.SaveChangeAsync();
        }
    }
}
