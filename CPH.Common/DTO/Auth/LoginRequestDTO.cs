using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Auth
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Please input username")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please input password")]
        public string Password { get; set; }
    }
}
