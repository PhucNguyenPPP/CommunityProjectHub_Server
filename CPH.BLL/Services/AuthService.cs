using AutoMapper;
using CPH.BLL.Interfaces;
using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using CPH.Common.Enum;
using CPH.DAL.Entities;
using CPH.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _config;

        public AuthService(IAccountService accountService, IUnitOfWork unitOfWork,
            IMapper mapper, IConfiguration configuration)
        {

            _accountService = accountService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
        }
        public async Task<bool> SignUp(SignUpRequestDTO model)
        {
            var account = _mapper.Map<Account>(model);

            var salt = _accountService.GenerateSalt();
            var passwordHash = _accountService.GenerateHashedPassword(model.Password, salt);
            //var avatarLink = await _imageService.StoreImageAndGetLink(model.AvatarLink, FileNameFirebaseStorage.UserImage);

            account.AccountId = Guid.NewGuid();
            account.AccountCode = _accountService.GenerateAccountCode(model.RoleId);
            account.Salt = salt;
            account.PasswordHash = passwordHash;
            account.Status = true;

            await _unitOfWork.Account.AddAsync(account);
            return await _unitOfWork.SaveChangeAsync();
        }

        public async Task<ResponseDTO> CheckValidationSignUp(SignUpRequestDTO model)
        {
            if (model.DateOfBirth >= DateTime.Now)
            {
                return new ResponseDTO("Date of birth is invalid", 400, false);
            }

            if (model.Gender != GenderEnum.Male.ToString()
                && model.Gender != GenderEnum.Female.ToString())
            {
                return new ResponseDTO("Gender is invalid", 400, false);
            }

            var checkAccountNameExist = _accountService.CheckAccountNameExist(model.AccountName);
            if (checkAccountNameExist)
            {
                return new ResponseDTO("Username already exists", 400, false);
            }

            var checkEmailExist = _accountService.CheckEmailExist(model.Email);
            if (checkEmailExist)
            {
                return new ResponseDTO("Email already exists", 400, false);
            }

            var checkPhoneExist = _accountService.CheckPhoneExist(model.Phone);
            if (checkPhoneExist)
            {
                return new ResponseDTO("Phone already exists", 400, false);
            }
            return new ResponseDTO("Check successfully", 200, true);
        }

        public async Task<LoginResponseDTO?> CheckLogin(LoginRequestDTO loginRequestDTO)
        {
            var user = _unitOfWork.Account.GetAllByCondition(x => x.AccountName == loginRequestDTO.UserName)
                .Include(u => u.Role).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            if (VerifyPasswordHash(loginRequestDTO.Password, user.PasswordHash, user.Salt))
            {
                string jwtTokenId = $"JTI{Guid.NewGuid()}";

                string refreshToken = await CreateNewRefreshToken(user.AccountId, jwtTokenId);

                var refreshTokenValid = _unitOfWork.RefreshToken
                    .GetAllByCondition(a => a.AccountId == user.AccountId
                    && a.RefreshToken1 != refreshToken)
                    .ToList();

                foreach (var token in refreshTokenValid)
                {
                    token.IsValid = false;
                }

                _unitOfWork.RefreshToken.UpdateRange(refreshTokenValid);
                await _unitOfWork.SaveChangeAsync();

                var accessToken = CreateToken(user, jwtTokenId);

                return new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            };
            return null;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHashDb, byte[] salt)
        {
            var passwordHash = _accountService.GenerateHashedPassword(password, salt);
            bool areEqual = passwordHashDb.SequenceEqual(passwordHash);
            return areEqual;
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private async Task<string> CreateNewRefreshToken(Guid accountId, string jwtId)
        {
            RefreshToken refreshAccessToken = new()
            {
                RefreshTokenId = Guid.NewGuid(),
                AccountId = accountId,
                JwtId = jwtId,
                ExpiredAt = DateTime.Now.AddHours(24),
                IsValid = true,
                RefreshToken1 = CreateRandomToken(),
            };
            await _unitOfWork.RefreshToken.AddAsync(refreshAccessToken);
            await _unitOfWork.SaveChangeAsync();
            return refreshAccessToken.RefreshToken1;
        }

        private string CreateToken(Account account, string jwtId)
        {
            var roleName = account.Role.RoleName;


            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.FullName.ToString()),
                new Claim(ClaimTypes.Role, roleName.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(JwtRegisteredClaimNames.Sub, account.AccountId.ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.AddMinutes(30).ToString(), ClaimValueTypes.Integer64)
            };

            var key = _config.GetSection("ApiSetting")["Secret"];
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(key ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               claims: claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: credentials
           );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<TokenDTO> RefreshAccessToken(RequestTokenDTO model)
        {
            // Find an existing refresh token
            var existingRefreshToken = _unitOfWork.RefreshToken
                .GetAllByCondition(r => r.RefreshToken1 == model.RefreshToken)
                .FirstOrDefault();

            if (existingRefreshToken == null)
            {
                return new TokenDTO()
                {
                    Message = "Token is not exists"
                };
            }

            // Compare data from exixsting refresh and access token provided and if there is any missmatch then consider it as fraud
            var isTokenValid = GetAccessTokenData(model.AccessToken, existingRefreshToken.AccountId, existingRefreshToken.JwtId);
            if (!isTokenValid)
            {
                existingRefreshToken.IsValid = false;
                await _unitOfWork.SaveChangeAsync();
                return new TokenDTO()
                {
                    Message = "Token is invalid"
                };
            }

            // Check accessToken expire ?
            var tokenHandler = new JwtSecurityTokenHandler();
            var test = tokenHandler.ReadJwtToken(model.AccessToken);
            if (test == null) return new TokenDTO()
            {
                Message = "Error when creating token"
            };

            var accessExpiredDateTime = test.ValidTo;
            // Sử dụng accessExpiredDateTime làm giá trị thời gian hết hạn

            if (accessExpiredDateTime > DateTime.UtcNow)
            {
                return new TokenDTO()
                {
                    Message = "Token is not expired"
                };
            }
            // When someone tries to use not valid refresh token, fraud possible

            if (!existingRefreshToken.IsValid)
            {
                var chainRecords = _unitOfWork.RefreshToken
                    .GetAllByCondition(u => u.AccountId == existingRefreshToken.AccountId
                    && u.JwtId == existingRefreshToken.JwtId)
                    .ToList();

                foreach (var item in chainRecords)
                {
                    item.IsValid = false;
                }
                _unitOfWork.RefreshToken.UpdateRange(chainRecords);
                await _unitOfWork.SaveChangeAsync();
                return new TokenDTO
                {
                    Message = "Token is invalid"
                };
            }

            // If it just expired then mark as invalid and return empty

            if (existingRefreshToken.ExpiredAt < DateTime.Now)
            {
                existingRefreshToken.IsValid = false;
                await _unitOfWork.SaveChangeAsync();
                return new TokenDTO()
                {
                    Message = "Token is expired"
                };
            }

            // Replace old refresh with a new one with updated expired date
            var newRefreshToken = await ReNewRefreshToken(existingRefreshToken.AccountId,
                existingRefreshToken.JwtId);

            // Revoke existing refresh token
            existingRefreshToken.IsValid = false;
            await _unitOfWork.SaveChangeAsync();
            // Generate new access token
            var user = _unitOfWork.Account.GetAllByCondition(a => a.AccountId == existingRefreshToken.AccountId)
                .FirstOrDefault();

            if (user == null)
            {
                return new TokenDTO();
            }

            var newAccessToken = CreateToken(user, existingRefreshToken.JwtId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Create token successfully"
            };
        }

        private bool GetAccessTokenData(string accessToken, Guid expectedAccountId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtId = jwt.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var accountId = jwt.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Sub)?.Value;
                accountId = accountId ?? string.Empty;
                return Guid.Parse(accountId) == expectedAccountId && jwtId == expectedTokenId;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> ReNewRefreshToken(Guid accountId, string jwtId)
        {
            var time = _unitOfWork.RefreshToken.GetAllByCondition(a => a.JwtId == jwtId)
                .FirstOrDefault();
            RefreshToken refreshAccessToken = new()
            {
                RefreshTokenId = Guid.NewGuid(),
                AccountId = accountId,
                JwtId = jwtId,
                ExpiredAt = time?.ExpiredAt != null ? time.ExpiredAt : DateTime.Now.AddHours(24),
                IsValid = true,
                RefreshToken1 = CreateRandomToken(),
            };
            await _unitOfWork.RefreshToken.AddAsync(refreshAccessToken);
            await _unitOfWork.SaveChangeAsync();
            return refreshAccessToken.RefreshToken1;
        }

        public async Task<ResponseDTO> GetAccountByAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new ResponseDTO("Token is empty", 400, false);
            }
            var accountId = ExtractAccountIdFromToken(accessToken);
            if (accountId == "Token is expired")
            {
                return new ResponseDTO("Token is expired", 400, false);
            }
            if (accountId == "Token is invalid")
            {
                return new ResponseDTO("Token is invalid", 400, false);
            }
            var user = await _unitOfWork.Account.GetAllByCondition(c => c.AccountId.ToString() == accountId
            && c.Status == true).Include(c => c.Role).FirstOrDefaultAsync();
            if (user == null)
            {
                return new ResponseDTO("User not found", 400, false);
            }
            var mapUser = _mapper.Map<LocalAccountDTO>(user);
            return new ResponseDTO("Get user by token successfully", 200, true, mapUser);
        }

        private string ExtractAccountIdFromToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(accessToken);

                //token expired
                var expiration = jwtToken.ValidTo;

                if (expiration < DateTime.UtcNow)
                {
                    return "Token is expired";
                }
                else
                {
                    string userId = jwtToken.Subject;
                    return userId;
                }
            }
            catch (Exception ex)
            {
                return "Token is invalid";
            }
        }

        public async Task<bool> LogOut(string refreshToken)
        {
            var refreshTokenDb = await _unitOfWork.RefreshToken.GetByCondition(c => c.RefreshToken1 == refreshToken);

            if (refreshTokenDb != null)
            {
                refreshTokenDb.IsValid = false;

                _unitOfWork.RefreshToken.Update(refreshTokenDb);
                return await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                return false;
            }
        }
}
}
