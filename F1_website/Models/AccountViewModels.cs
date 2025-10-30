using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace F1_website.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }


    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome de utilizador deve ter no máximo 50 caracteres")]
        [Display(Name = "Nome de Utilizador")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(100, ErrorMessage = "A password deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Password")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "A morada é obrigatória")]
        [StringLength(200, ErrorMessage = "A morada deve ter no máximo 200 caracteres")]
        [Display(Name = "Morada")]
        public string Morada { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [Display(Name = "Foto")]
        public string Foto { get; set; }

        [NotMapped]
        public HttpPostedFileBase FotoFile { get; set; }

        public int Idade
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DataNascimento.Year;
                if (DataNascimento > today.AddYears(-age)) age--;
                return age;
            }
        }
    }


    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class PainelViewModel
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Morada { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}
