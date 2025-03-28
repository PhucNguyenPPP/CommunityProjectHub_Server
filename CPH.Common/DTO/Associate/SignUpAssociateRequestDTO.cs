﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Associate
{
    public class SignUpAssociateRequestDTO
    {
        [Required(ErrorMessage = "Please input account name")]
        [MinLength(5, ErrorMessage = "Account name must have at least 5 characters")]
        public string AccountName { get; set; } = null!;

        [Required(ErrorMessage = "Please input full name")]
        [MinLength(8, ErrorMessage = "Full name must have at least 8 characters")]
        [RegularExpression("^[\\p{L}]+([\\s\\p{L}]+)*$",
            ErrorMessage = "Full name is invalid")]
        public string FullName { get; set; } = null!;

        public IFormFile? AvatarLink { get; set; } = null!;

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

        [Required(ErrorMessage = "Please input associate name")]
        public string AssociateName { get; set; } = null!;
    }
}
