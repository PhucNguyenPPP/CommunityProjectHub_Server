using CPH.Common.DTO.Auth;
using CPH.Common.DTO.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<bool> SignUp(SignUpRequestDTO model);
        Task<ResponseDTO> CheckValidationSignUp(SignUpRequestDTO model);
        byte[] GenerateSalt();
        byte[] GenerateHashedPassword(string password, byte[] saltBytes);
        Task<LoginResponseDTO?> CheckLogin(LoginRequestDTO loginRequestDTO);
        Task<TokenDTO> RefreshAccessToken(RequestTokenDTO model);
        Task<ResponseDTO> GetAccountByAccessToken(string token);
        Task<bool> LogOut(string refreshToken);
    }
}
