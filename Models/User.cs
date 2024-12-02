using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace ShiftLogger.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Jméno je vyžadováno.")]
        public override string UserName { get; set; }

        
        [Required(ErrorMessage = "Email je vyžadován.")]
        [CustomEmailValidation(ErrorMessage = "Neplatný formát emailu.")]
        public override string Email { get; set; }
        

        [NotMapped]
        public string? Pass { get; set; }
        public UserRole Role { get; set; }
        public bool MustChangePassword { get; set; }

        public enum UserRole
        {
            Admin = 0,
            Driver = 1
        }

        public class CustomEmailValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value is string email)
                {
                    var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                    if (!regex.IsMatch(email))
                        return new ValidationResult(ErrorMessage);
                }
                return ValidationResult.Success;
            }
        }
    }
}
