using F1_website.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace F1_website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            var model = new paginacomunicaçãoviewmodel
            {
                Login = new LoginViewModel(),
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel(),
                Equipas = new BD().GetEquipas(),
                Pilotos = new BD().GetPilotos()
            };

            return View(model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(paginacomunicaçãoviewmodel model, string returnUrl)
        {
            if (!ModelState.IsValid || model.Login == null)
            {
                // Recarregar os dados que a view espera
                model.Email = new EmailViewModel();
                model.Newsletter = new NewsletterViewModel();
                model.Equipas = new BD().GetEquipas();
                model.Pilotos = new BD().GetPilotos();

                return View(model);
            }

            // Autenticação com ASP.NET Identity
            var result = await SignInManager.PasswordSignInAsync(
                model.Login.UserName,
                model.Login.Password,
                model.Login.RememberMe,
                shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Painel", "Account");

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new
                    {
                        ReturnUrl = returnUrl,
                        RememberMe = model.Login.RememberMe
                    });

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Tentativa de login inválida.");

                    // Recarregar os dados que a view espera
                    model.Email = new EmailViewModel();
                    model.Newsletter = new NewsletterViewModel();
                    model.Equipas = new BD().GetEquipas();
                    model.Pilotos = new BD().GetPilotos();

                    return View(model);
            }
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new paginacomunicaçãoviewmodel
            {
                Register = new RegisterViewModel(),
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel(),
                Equipas = new BD().GetEquipas(),
                Pilotos = new BD().GetPilotos()
            };

            return View(model);
        }

        //
        // POST: /Account/Register
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(paginacomunicaçãoviewmodel model, HttpPostedFileBase FotoFile)
        {
            if (model.Register != null)
            {
                // Validar data de nascimento
                if (model.Register.DataNascimento == null)
                {
                    ModelState.AddModelError("Register.DataNascimento", "A data de nascimento é obrigatória.");
                }
                else
                {
                    var hoje = DateTime.Now.Date;
                    var idadeMinima = hoje.AddYears(-120); // 120 anos máximo
                    var idadeMaxima = hoje.AddYears(-13);  // 13 anos mínimo

                    if (model.Register.DataNascimento > idadeMaxima)
                    {
                        ModelState.AddModelError("Register.DataNascimento", "Deve ter pelo menos 13 anos para se registar.");
                    }
                    else if (model.Register.DataNascimento < idadeMinima)
                    {
                        ModelState.AddModelError("Register.DataNascimento", "Data de nascimento inválida.");
                    }
                }

                // Validar nome (sem números nem caracteres especiais)
                if (!string.IsNullOrEmpty(model.Register.Nome))
                {
                    if (model.Register.Nome.Any(c => char.IsDigit(c)))
                    {
                        ModelState.AddModelError("Register.Nome", "O nome não pode conter números.");
                    }
                    if (model.Register.Nome.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Register.Nome", "O nome contém caracteres inválidos.");
                    }
                    if (model.Register.Nome.Length > 100)
                    {
                        ModelState.AddModelError("Register.Nome", "O nome não pode ter mais de 100 caracteres.");
                    }
                }

                // Validar username (sem espaços, caracteres especiais perigosos)
                if (!string.IsNullOrEmpty(model.Register.UserName))
                {
                    if (model.Register.UserName.Contains(" "))
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador não pode conter espaços.");
                    }
                    if (model.Register.UserName.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador contém caracteres inválidos.");
                    }
                    if (model.Register.UserName.Length < 3)
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador deve ter pelo menos 3 caracteres.");
                    }
                    if (model.Register.UserName.Length > 50)
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador não pode ter mais de 50 caracteres.");
                    }
                }

                // Validar email formato
                if (!string.IsNullOrEmpty(model.Register.Email))
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(model.Register.Email);
                        if (addr.Address != model.Register.Email)
                        {
                            ModelState.AddModelError("Register.Email", "Formato de email inválido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("Register.Email", "Formato de email inválido.");
                    }
                }

                // Validar morada
                if (!string.IsNullOrEmpty(model.Register.Morada))
                {
                    if (model.Register.Morada.Length > 200)
                    {
                        ModelState.AddModelError("Register.Morada", "A morada não pode ter mais de 200 caracteres.");
                    }
                }

                // Validar password
                if (!string.IsNullOrEmpty(model.Register.Password))
                {
                    if (model.Register.Password.Length < 6)
                    {
                        ModelState.AddModelError("Register.Password", "A password deve ter pelo menos 6 caracteres.");
                    }
                    if (!model.Register.Password.Any(char.IsUpper))
                    {
                        ModelState.AddModelError("Register.Password", "A password deve conter pelo menos uma letra maiúscula.");
                    }
                    if (!model.Register.Password.Any(char.IsDigit))
                    {
                        ModelState.AddModelError("Register.Password", "A password deve conter pelo menos um número.");
                    }
                }

                // Validar ficheiro de foto
                if (FotoFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(FotoFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Apenas são permitidos ficheiros de imagem (JPG, PNG, GIF).");
                    }

                    if (FotoFile.ContentLength > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("", "O ficheiro de foto não pode ser superior a 5MB.");
                    }
                }
            }

            if (ModelState.IsValid && model.Register != null)
            {
                // Processar upload da foto se existir
                string fotoPath = null;

                if (FotoFile != null && FotoFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/imagens/UserImages/");
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        FotoFile.SaveAs(filePath);
                        fotoPath = "~/imagens/UserImages/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Erro ao fazer upload da foto: " + ex.Message);
                        return View(model);
                    }
                }

                // Verificar se o email já existe
                var existingUser = await UserManager.FindByEmailAsync(model.Register.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Este email já está registado.");
                    return View(model);
                }

                // Verificar se o username já existe
                var existingUsername = await UserManager.FindByNameAsync(model.Register.UserName);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("", "Este nome de utilizador já está ocupado.");
                    return View(model);
                }

                // Criar o utilizador
                var user = new ApplicationUser
                {
                    UserName = model.Register.UserName,
                    Email = model.Register.Email,
                    Nome = model.Register.Nome,
                    Morada = model.Register.Morada,
                    DataNascimento = model.Register.DataNascimento,
                    Foto = fotoPath,
                    EmailConfirmed = true
                };

                var result = await UserManager.CreateAsync(user, model.Register.Password);

                if (result.Succeeded)
                {
                    var createdUser = await UserManager.FindByEmailAsync(model.Register.Email);
                    if (createdUser != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Painel", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Erro ao criar utilizador na base de dados.");
                    }
                }
                else
                {
                    AddErrors(result);
                }
            }

            // Recarregar dados da view se algo falhou
            model.Email = new EmailViewModel();
            model.Newsletter = new NewsletterViewModel();
            model.Equipas = new BD().GetEquipas();
            model.Pilotos = new BD().GetPilotos();

            return View(model);
        }


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        [Authorize]
        public ActionResult Painel()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login");

                var model = new paginacomunicaçãoviewmodel
                {
                    Painel = new PainelViewModel
                    {
                        Nome = user.Nome,
                        Email = user.Email,
                        Morada = user.Morada,
                        DataNascimento = user.DataNascimento
                    },
                    Newsletter = new NewsletterViewModel(),
                    Email = new EmailViewModel(),
                    Equipas = new BD().GetEquipas(),
                    Pilotos = new BD().GetPilotos()
                };

                return View(model);
            }
        }
        [Authorize]
        public ActionResult EditarDados()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login");

                var model = new paginacomunicaçãoviewmodel
                {
                    Painel = new PainelViewModel
                    {
                        Nome = user.Nome,
                        Email = user.Email,
                        Morada = user.Morada,
                        DataNascimento = user.DataNascimento
                    }
                };
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditarDados(paginacomunicaçãoviewmodel model)
        {
            if (model.Painel != null)
            {
                // Validar data de nascimento
                if (model.Painel.DataNascimento == null)
                {
                    ModelState.AddModelError("Painel.DataNascimento", "A data de nascimento é obrigatória.");
                }
                else
                {
                    var hoje = DateTime.Now.Date;
                    var idadeMinima = hoje.AddYears(-120);
                    var idadeMaxima = hoje.AddYears(-13);

                    if (model.Painel.DataNascimento > idadeMaxima)
                    {
                        ModelState.AddModelError("Painel.DataNascimento", "Deve ter pelo menos 13 anos.");
                    }
                    else if (model.Painel.DataNascimento < idadeMinima)
                    {
                        ModelState.AddModelError("Painel.DataNascimento", "Data de nascimento inválida.");
                    }
                }

                // Validar nome
                if (!string.IsNullOrEmpty(model.Painel.Nome))
                {
                    if (model.Painel.Nome.Any(c => char.IsDigit(c)))
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome não pode conter números.");
                    }
                    if (model.Painel.Nome.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome contém caracteres inválidos.");
                    }
                    if (model.Painel.Nome.Length > 100)
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome não pode ter mais de 100 caracteres.");
                    }
                }

                // Validar email
                if (!string.IsNullOrEmpty(model.Painel.Email))
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(model.Painel.Email);
                        if (addr.Address != model.Painel.Email)
                        {
                            ModelState.AddModelError("Painel.Email", "Formato de email inválido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("Painel.Email", "Formato de email inválido.");
                    }
                }

                // Validar morada
                if (!string.IsNullOrEmpty(model.Painel.Morada))
                {
                    if (model.Painel.Morada.Length > 200)
                    {
                        ModelState.AddModelError("Painel.Morada", "A morada não pode ter mais de 200 caracteres.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login");

                // Verificar se o email já existe (exceto para o próprio utilizador)
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.Painel.Email && u.Id != userId);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Painel.Email", "Este email já está a ser utilizado por outro utilizador.");
                    return View(model);
                }

                user.Nome = model.Painel.Nome;
                user.Email = model.Painel.Email;
                user.Morada = model.Painel.Morada;
                user.DataNascimento = model.Painel.DataNascimento;

                db.SaveChanges();
            }
            return RedirectToAction("Painel");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EliminarConta()
        {
            var userId = User.Identity.GetUserId();

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    db.Users.Remove(user);
                    db.SaveChanges();
                }
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Register");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }
        [Authorize]
        public ActionResult GestaoRoles()
        {
            using (var db = new ApplicationDbContext())
            {
                var users = db.Users.ToList();
                var roles = db.Roles.ToList();

                var usersWithRoles = new List<UserRoleViewModel>();

                foreach (var user in users)
                {
                    var userRoles = user.Roles.FirstOrDefault();
                    var roleName = userRoles != null ?
                        roles.FirstOrDefault(r => r.Id == userRoles.RoleId)?.Name :
                        "Sem Cargo";

                    usersWithRoles.Add(new UserRoleViewModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Nome = user.Nome,
                        RoleName = roleName,
                        RoleId = userRoles?.RoleId,
                        HasRole = userRoles != null
                    });
                }

                // Converter IdentityRole para AspNetRoles
                var aspNetRoles = roles.Select(r => new F1_website.Models.AspNetRoles
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                var model = new paginacomunicaçãoviewmodel
                {
                    UsersWithRoles = usersWithRoles,
                    RolesEF = aspNetRoles, // Usar a lista convertida
                    RolesList = roles.Select(r => new SelectListItem
                    {
                        Value = r.Id,
                        Text = r.Name
                    }).ToList(),
                    Newsletter = new NewsletterViewModel(),
                    Email = new EmailViewModel(),
                    Equipas = new BD().GetEquipas(),
                    Pilotos = new BD().GetPilotos()
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AtribuirRole(string userId, string roleId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Utilizador não encontrado." });
                    }

                    var role = db.Roles.Find(roleId);
                    if (role == null)
                    {
                        return Json(new { success = false, message = "Cargo não encontrado." });
                    }

                    // Remove role anterior se existir
                    var existingUserRole = user.Roles.FirstOrDefault();
                    if (existingUserRole != null)
                    {
                        user.Roles.Remove(existingUserRole);
                    }

                    // Adiciona novo role
                    var newUserRole = new IdentityUserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };
                    user.Roles.Add(newUserRole);

                    db.SaveChanges();
                    return Json(new { success = true, message = "Cargo atribuído com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult RemoverRole(string userId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Utilizador não encontrado." });
                    }

                    var existingRole = user.Roles.FirstOrDefault();
                    if (existingRole != null)
                    {
                        user.Roles.Remove(existingRole);
                        db.SaveChanges();
                    }

                    return Json(new { success = true, message = "Cargo removido com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CriarRole(string nomeCargo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeCargo))
                {
                    return Json(new { success = false, message = "Nome do cargo é obrigatório." });
                }

                // Validações do nome do cargo
                if (nomeCargo.Length > 50)
                {
                    return Json(new { success = false, message = "O nome do cargo não pode ter mais de 50 caracteres." });
                }

                if (nomeCargo.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                {
                    return Json(new { success = false, message = "O nome do cargo contém caracteres inválidos." });
                }

                if (nomeCargo.Any(char.IsDigit))
                {
                    return Json(new { success = false, message = "O nome do cargo não pode conter números." });
                }

                using (var db = new ApplicationDbContext())
                {
                    // Verifica se já existe (case insensitive)
                    var existingRole = db.Roles.FirstOrDefault(r => r.Name.ToLower() == nomeCargo.ToLower());
                    if (existingRole != null)
                    {
                        return Json(new { success = false, message = "Já existe um cargo com este nome." });
                    }

                    var newRole = new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = nomeCargo.Trim()
                    };

                    db.Roles.Add(newRole);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Cargo criado com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EliminarRole(string roleId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var role = db.Roles.Find(roleId);
                    if (role == null)
                    {
                        return Json(new { success = false, message = "Cargo não encontrado." });
                    }

                    // Verifica se há utilizadores com este cargo
                    var usersWithRole = db.Users.Where(u => u.Roles.Any(ur => ur.RoleId == roleId)).ToList();
                    if (usersWithRole.Any())
                    {
                        return Json(new { success = false, message = "Não é possível eliminar. Existem utilizadores com este cargo." });
                    }

                    db.Roles.Remove(role);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Cargo eliminado com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        
        [AllowAnonymous]
        public ActionResult Login2(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            var model = new paginacomunicaçãoviewmodel
            {
                Login = new LoginViewModel(),
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel(),
                Equipas = new BD().GetEquipas(),
                Pilotos = new BD().GetPilotos()
            };

            return View(model);
        }

        //
        // POST: /Account/Login2
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login2(paginacomunicaçãoviewmodel model, string returnUrl)
        {
            if (!ModelState.IsValid || model.Login == null)
            {
                // Recarregar os dados que a view espera
                model.Email = new EmailViewModel();
                model.Newsletter = new NewsletterViewModel();
                model.Equipas = new BD().GetEquipas();
                model.Pilotos = new BD().GetPilotos();

                return View(model);
            }

            // Autenticação com ASP.NET Identity
            var result = await SignInManager.PasswordSignInAsync(
                model.Login.UserName,
                model.Login.Password,
                model.Login.RememberMe,
                shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Painel2", "Account");

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new
                    {
                        ReturnUrl = returnUrl,
                        RememberMe = model.Login.RememberMe
                    });

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Tentativa de login inválida.");

                    // Recarregar os dados que a view espera
                    model.Email = new EmailViewModel();
                    model.Newsletter = new NewsletterViewModel();
                    model.Equipas = new BD().GetEquipas();
                    model.Pilotos = new BD().GetPilotos();

                    return View(model);
            }
        }

        //
        // GET: /Account/Painel2
        [Authorize]
        public ActionResult Painel2()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login2");

                var model = new paginacomunicaçãoviewmodel
                {
                    Painel = new PainelViewModel
                    {
                        Nome = user.Nome,
                        Email = user.Email,
                        Morada = user.Morada,
                        DataNascimento = user.DataNascimento
                    },
                    Newsletter = new NewsletterViewModel(),
                    Email = new EmailViewModel(),
                    Equipas = new BD().GetEquipas(),
                    Pilotos = new BD().GetPilotos()
                };

                return View(model);
            }
        }

        //
        // GET: /Account/EditarDados2
        [Authorize]
        public ActionResult EditarDados2()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login2");

                var model = new paginacomunicaçãoviewmodel
                {
                    Painel = new PainelViewModel
                    {
                        Nome = user.Nome,
                        Email = user.Email,
                        Morada = user.Morada,
                        DataNascimento = user.DataNascimento
                    }
                };
                return View(model);
            }
        }

        //
        // POST: /Account/EditarDados2
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditarDados2(paginacomunicaçãoviewmodel model)
        {
            // Aplicar as mesmas validações do EditarDados
            if (model.Painel != null)
            {
                // Validar data de nascimento
                if (model.Painel.DataNascimento == null)
                {
                    ModelState.AddModelError("Painel.DataNascimento", "A data de nascimento é obrigatória.");
                }
                else
                {
                    var hoje = DateTime.Now.Date;
                    var idadeMinima = hoje.AddYears(-120);
                    var idadeMaxima = hoje.AddYears(-13);

                    if (model.Painel.DataNascimento > idadeMaxima)
                    {
                        ModelState.AddModelError("Painel.DataNascimento", "Deve ter pelo menos 13 anos.");
                    }
                    else if (model.Painel.DataNascimento < idadeMinima)
                    {
                        ModelState.AddModelError("Painel.DataNascimento", "Data de nascimento inválida.");
                    }
                }

                // Validar nome
                if (!string.IsNullOrEmpty(model.Painel.Nome))
                {
                    if (model.Painel.Nome.Any(c => char.IsDigit(c)))
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome não pode conter números.");
                    }
                    if (model.Painel.Nome.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome contém caracteres inválidos.");
                    }
                    if (model.Painel.Nome.Length > 100)
                    {
                        ModelState.AddModelError("Painel.Nome", "O nome não pode ter mais de 100 caracteres.");
                    }
                }

                // Validar email
                if (!string.IsNullOrEmpty(model.Painel.Email))
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(model.Painel.Email);
                        if (addr.Address != model.Painel.Email)
                        {
                            ModelState.AddModelError("Painel.Email", "Formato de email inválido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("Painel.Email", "Formato de email inválido.");
                    }
                }

                // Validar morada
                if (!string.IsNullOrEmpty(model.Painel.Morada))
                {
                    if (model.Painel.Morada.Length > 200)
                    {
                        ModelState.AddModelError("Painel.Morada", "A morada não pode ter mais de 200 caracteres.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.Identity.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return RedirectToAction("Login2");

                // Verificar se o email já existe (exceto para o próprio utilizador)
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.Painel.Email && u.Id != userId);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Painel.Email", "Este email já está a ser utilizado por outro utilizador.");
                    return View(model);
                }

                user.Nome = model.Painel.Nome;
                user.Email = model.Painel.Email;
                user.Morada = model.Painel.Morada;
                user.DataNascimento = model.Painel.DataNascimento;

                db.SaveChanges();
            }
            return RedirectToAction("Painel2");
        }


        //
        // GET: /Account/GestaoRoles2
        [Authorize]
        public ActionResult GestaoRoles2()
        {
            using (var db = new ApplicationDbContext())
            {
                var users = db.Users.ToList();
                var roles = db.Roles.ToList();

                var usersWithRoles = new List<UserRoleViewModel>();

                foreach (var user in users)
                {
                    var userRoles = user.Roles.FirstOrDefault();
                    var roleName = userRoles != null ?
                        roles.FirstOrDefault(r => r.Id == userRoles.RoleId)?.Name :
                        "Sem Cargo";

                    usersWithRoles.Add(new UserRoleViewModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Nome = user.Nome,
                        RoleName = roleName,
                        RoleId = userRoles?.RoleId,
                        HasRole = userRoles != null
                    });
                }

                // Converter IdentityRole para AspNetRoles
                var aspNetRoles = roles.Select(r => new F1_website.Models.AspNetRoles
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();

                var model = new paginacomunicaçãoviewmodel
                {
                    UsersWithRoles = usersWithRoles,
                    RolesEF = aspNetRoles, // Usar a lista convertida
                    RolesList = roles.Select(r => new SelectListItem
                    {
                        Value = r.Id,
                        Text = r.Name
                    }).ToList(),
                    Newsletter = new NewsletterViewModel(),
                    Email = new EmailViewModel(),
                    Equipas = new BD().GetEquipas(),
                    Pilotos = new BD().GetPilotos()
                };

                return View(model);
            }
        }

        //
        // POST: /Account/AtribuirRole2
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AtribuirRole2(string userId, string roleId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Utilizador não encontrado." });
                    }

                    var role = db.Roles.Find(roleId);
                    if (role == null)
                    {
                        return Json(new { success = false, message = "Cargo não encontrado." });
                    }

                    // Remove role anterior se existir
                    var existingUserRole = user.Roles.FirstOrDefault();
                    if (existingUserRole != null)
                    {
                        user.Roles.Remove(existingUserRole);
                    }

                    // Adiciona novo role
                    var newUserRole = new IdentityUserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };
                    user.Roles.Add(newUserRole);

                    db.SaveChanges();
                    return Json(new { success = true, message = "Cargo atribuído com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }

        //
        // POST: /Account/RemoverRole2
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult RemoverRole2(string userId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Utilizador não encontrado." });
                    }

                    var existingRole = user.Roles.FirstOrDefault();
                    if (existingRole != null)
                    {
                        user.Roles.Remove(existingRole);
                        db.SaveChanges();
                    }

                    return Json(new { success = true, message = "Cargo removido com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CriarRole2(string nomeCargo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeCargo))
                {
                    return Json(new { success = false, message = "Nome do cargo é obrigatório." });
                }

                // Validações do nome do cargo
                if (nomeCargo.Length > 50)
                {
                    return Json(new { success = false, message = "O nome do cargo não pode ter mais de 50 caracteres." });
                }

                if (nomeCargo.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                {
                    return Json(new { success = false, message = "O nome do cargo contém caracteres inválidos." });
                }

                if (nomeCargo.Any(char.IsDigit))
                {
                    return Json(new { success = false, message = "O nome do cargo não pode conter números." });
                }

                using (var db = new ApplicationDbContext())
                {
                    // Verifica se já existe (case insensitive)
                    var existingRole = db.Roles.FirstOrDefault(r => r.Name.ToLower() == nomeCargo.ToLower());
                    if (existingRole != null)
                    {
                        return Json(new { success = false, message = "Já existe um cargo com este nome." });
                    }

                    var newRole = new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = nomeCargo.Trim()
                    };

                    db.Roles.Add(newRole);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Cargo criado com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }

        //
        // POST: /Account/EliminarRole2
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EliminarRole2(string roleId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var role = db.Roles.Find(roleId);
                    if (role == null)
                    {
                        return Json(new { success = false, message = "Cargo não encontrado." });
                    }

                    // Verifica se há utilizadores com este cargo
                    var usersWithRole = db.Users.Where(u => u.Roles.Any(ur => ur.RoleId == roleId)).ToList();
                    if (usersWithRole.Any())
                    {
                        return Json(new { success = false, message = "Não é possível eliminar. Existem utilizadores com este cargo." });
                    }

                    db.Roles.Remove(role);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Cargo eliminado com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro: " + ex.Message });
            }
        }
        //
        // GET: /Account/Register2
        [AllowAnonymous]
        public ActionResult Register2()
        {
            var model = new paginacomunicaçãoviewmodel
            {
                Register = new RegisterViewModel(),
                Email = new EmailViewModel(),
                Newsletter = new NewsletterViewModel(),
                Equipas = new BD().GetEquipas(),
                Pilotos = new BD().GetPilotos()
            };

            return View(model);
        }

        //
        // POST: /Account/Register2
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register2(paginacomunicaçãoviewmodel model, HttpPostedFileBase FotoFile)
        {
            // Aplicar as mesmas validações do Register
            if (model.Register != null)
            {
                // Validar data de nascimento
                if (model.Register.DataNascimento == null)
                {
                    ModelState.AddModelError("Register.DataNascimento", "A data de nascimento é obrigatória.");
                }
                else
                {
                    var hoje = DateTime.Now.Date;
                    var idadeMinima = hoje.AddYears(-120);
                    var idadeMaxima = hoje.AddYears(-13);

                    if (model.Register.DataNascimento > idadeMaxima)
                    {
                        ModelState.AddModelError("Register.DataNascimento", "Deve ter pelo menos 13 anos para se registar.");
                    }
                    else if (model.Register.DataNascimento < idadeMinima)
                    {
                        ModelState.AddModelError("Register.DataNascimento", "Data de nascimento inválida.");
                    }
                }

                // Validar nome
                if (!string.IsNullOrEmpty(model.Register.Nome))
                {
                    if (model.Register.Nome.Any(c => char.IsDigit(c)))
                    {
                        ModelState.AddModelError("Register.Nome", "O nome não pode conter números.");
                    }
                    if (model.Register.Nome.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Register.Nome", "O nome contém caracteres inválidos.");
                    }
                    if (model.Register.Nome.Length > 100)
                    {
                        ModelState.AddModelError("Register.Nome", "O nome não pode ter mais de 100 caracteres.");
                    }
                }

                // Validar username
                if (!string.IsNullOrEmpty(model.Register.UserName))
                {
                    if (model.Register.UserName.Contains(" "))
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador não pode conter espaços.");
                    }
                    if (model.Register.UserName.Any(c => "!@#$%^&*()+=[]{}|\\:;\"'<>?/".Contains(c)))
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador contém caracteres inválidos.");
                    }
                    if (model.Register.UserName.Length < 3)
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador deve ter pelo menos 3 caracteres.");
                    }
                    if (model.Register.UserName.Length > 50)
                    {
                        ModelState.AddModelError("Register.UserName", "O nome de utilizador não pode ter mais de 50 caracteres.");
                    }
                }

                // Validar email
                if (!string.IsNullOrEmpty(model.Register.Email))
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(model.Register.Email);
                        if (addr.Address != model.Register.Email)
                        {
                            ModelState.AddModelError("Register.Email", "Formato de email inválido.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("Register.Email", "Formato de email inválido.");
                    }
                }

                // Validar morada
                if (!string.IsNullOrEmpty(model.Register.Morada))
                {
                    if (model.Register.Morada.Length > 200)
                    {
                        ModelState.AddModelError("Register.Morada", "A morada não pode ter mais de 200 caracteres.");
                    }
                }

                // Validar password
                if (!string.IsNullOrEmpty(model.Register.Password))
                {
                    if (model.Register.Password.Length < 6)
                    {
                        ModelState.AddModelError("Register.Password", "A password deve ter pelo menos 6 caracteres.");
                    }
                    if (!model.Register.Password.Any(char.IsUpper))
                    {
                        ModelState.AddModelError("Register.Password", "A password deve conter pelo menos uma letra maiúscula.");
                    }
                    if (!model.Register.Password.Any(char.IsDigit))
                    {
                        ModelState.AddModelError("Register.Password", "A password deve conter pelo menos um número.");
                    }
                }

                // Validar ficheiro de foto
                if (FotoFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(FotoFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Apenas são permitidos ficheiros de imagem (JPG, PNG, GIF).");
                    }

                    if (FotoFile.ContentLength > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("", "O ficheiro de foto não pode ser superior a 5MB.");
                    }
                }
            }

            if (ModelState.IsValid && model.Register != null)
            {
                // Processar upload da foto se existir
                string fotoPath = null;

                if (FotoFile != null && FotoFile.ContentLength > 0)
                {
                    try
                    {
                        var uploadDir = Server.MapPath("~/imagens/UserImages/");
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(FotoFile.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);

                        FotoFile.SaveAs(filePath);
                        fotoPath = "~/imagens/UserImages/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Erro ao fazer upload da foto: " + ex.Message);
                        return View(model);
                    }
                }

                // Verificar se o email já existe
                var existingUser = await UserManager.FindByEmailAsync(model.Register.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Este email já está registado.");
                    return View(model);
                }

                // Verificar se o username já existe
                var existingUsername = await UserManager.FindByNameAsync(model.Register.UserName);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("", "Este nome de utilizador já está ocupado.");
                    return View(model);
                }

                // Criar o utilizador
                var user = new ApplicationUser
                {
                    UserName = model.Register.UserName,
                    Email = model.Register.Email,
                    Nome = model.Register.Nome,
                    Morada = model.Register.Morada,
                    DataNascimento = model.Register.DataNascimento,
                    Foto = fotoPath,
                    EmailConfirmed = true
                };

                var result = await UserManager.CreateAsync(user, model.Register.Password);

                if (result.Succeeded)
                {
                    var createdUser = await UserManager.FindByEmailAsync(model.Register.Email);
                    if (createdUser != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Painel2", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Erro ao criar utilizador na base de dados.");
                    }
                }
                else
                {
                    AddErrors(result);
                }
            }

            model.Email = new EmailViewModel();
            model.Newsletter = new NewsletterViewModel();
            model.Equipas = new BD().GetEquipas();
            model.Pilotos = new BD().GetPilotos();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff2()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login2", "Account");
        }
        #endregion
    }
}