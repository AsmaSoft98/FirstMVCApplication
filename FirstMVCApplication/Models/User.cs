using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FirstMVCApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "User Name is required")]
        [StringLength(100)]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Email is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Wrong email size")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Email must be in user@domain.com format")]
        public string Email { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }
        //[Required]
        [StringLength(15)]
        // this is a simple example, phone number is mere complicated, we must use custom validation
        [RegularExpression(@"^[0-9 x.+)(-]{11,}$", ErrorMessage = "Phone Number must be numbers")] 
        public string PhoneNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public Guid ActivationCode { get; set; }
    }
}