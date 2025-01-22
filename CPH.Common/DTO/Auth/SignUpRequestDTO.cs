using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Auth
{
    public class SignUpRequestDTO
    {
        [Required(ErrorMessage = "Please input account name")]
        [MinLength(5, ErrorMessage = "Account name must have at least 5 characters")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Please input password")]
        [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
        [RegularExpression("^(?=.*[!@#$%^&*(),.?\":{}|<>]).+$",
            ErrorMessage = "Password must have at least 1 special character")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please input full name")]
        [MinLength(8, ErrorMessage = "Full name must have at least 8 characters")]
        [RegularExpression("^[\\p{L}]+([\\s\\p{L}]+)*$",
            ErrorMessage = "Full name is invalid")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Please choose avatar")]
        public IFormFile AvatarLink { get; set; } = null!;

        [Required(ErrorMessage = "Please input phone number")]
        [RegularExpression("^0\\d{9}$",
            ErrorMessage = "Phone number is invalid")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Please input address")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Please input email")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Please input date of birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please input gender")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Please input role id")]
        [Range(1,6)]
        public int RoleId { get; set; }

    }
}
