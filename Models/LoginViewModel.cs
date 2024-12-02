using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nesmí být prázdné")]
        [EmailAddress(ErrorMessage = "Email je ve špatném formátu.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Nesmí být prázdné")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
