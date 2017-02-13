using System.ComponentModel.DataAnnotations;

namespace SequenceAlignment.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Please, Enter your username.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please, Enter your Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please, Enter your Password.")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Please, Make sure your passwords are matched.")]
        public string ComparePassword { get; set; }
    }
}
