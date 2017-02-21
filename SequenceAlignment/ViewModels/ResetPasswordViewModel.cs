using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SequenceAlignment.ViewModels
{
    public class ResetPasswordViewModel
    {
        [HiddenInput]
        public string UserId { get; set; }

        [HiddenInput]
        public string CodeToken { get; set; }

        [Required(ErrorMessage ="Can't set password to empty")]
        public string Password { get; set; }

        [Compare(nameof(Password),ErrorMessage ="Make sure to make your passwords are matched")]
        public string RetypePassword { get; set; }
    }
}
