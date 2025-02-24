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
        Task<bool> SignUp2(SignUpRequestDTO2 model);
        Task<ResponseDTO> CheckValidationSignUp(SignUpRequestDTO model);
        Task<ResponseDTO> CheckValidationSignUp2(SignUpRequestDTO2 model);
        Task<LoginResponseDTO?> CheckLogin(LoginRequestDTO loginRequestDTO);
        Task<TokenDTO> RefreshAccessToken(RequestTokenDTO model);
        Task<ResponseDTO> GetAccountByAccessToken(string token);
        Task<bool> LogOut(string refreshToken);
        ResponseDTO CheckOldPassword(CheckOldPasswordDTO model);
    }
}
