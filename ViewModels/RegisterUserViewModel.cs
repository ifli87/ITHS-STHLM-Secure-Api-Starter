using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace secure_api.ViewModels
{
    public class RegisterUserViewModel
    {   
        [Required]
        [EmailAddress(ErrorMessage ="Felaktig e-post Adress ")]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        public bool IsAdmin { get; set; }=false;

    }
}