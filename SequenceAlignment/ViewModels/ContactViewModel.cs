using System.ComponentModel.DataAnnotations;

namespace SequenceAlignment.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Enter your UserName is required!")]
        public string UserName { get; set; }

        [Required(ErrorMessage ="Enter your Email is required!")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Enter your Message is required!")]
        public string Message { get; set; }

    }
}
