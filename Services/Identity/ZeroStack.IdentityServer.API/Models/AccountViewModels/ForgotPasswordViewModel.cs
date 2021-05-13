﻿using System.ComponentModel.DataAnnotations;

namespace ZeroStack.IdentityServer.API.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Confirm Password", Prompt = "Confirm Password")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "The {0} field is required."), Phone]
        [Display(Name = "Phone Number", Prompt = "Phone Number")]
        [RegularExpression(@"^1\d{10}$", ErrorMessage = "The {0} must be 11 digits.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Confirmed Code", Prompt = "Confirmed Code")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The {0} must be 6 digits.")]
        public string ConfirmedCode { get; set; } = null!;
    }
}
