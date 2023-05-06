using EntityFrameWork.Models;
using EntityFrameWork.Services.Interfaces;
using EntityFrameWork.ViewModels.Account;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace EntityFrameWork.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Register(RegisterVM model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AppUser newUser = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName

            };

            IdentityResult result = await _userManager.CreateAsync(newUser, model.PassWord);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(model);

            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            string link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = newUser.Id , token  },Request.Scheme,Request.Host.ToString());

            string subject = "Register Confirmation";

            string html = String.Empty;

            using (StreamReader reader=new StreamReader("wwwroot/templates/verify.html"))
            {
                html = reader.ReadToEnd();
            }

            html = html.Replace("{{link}}", link);
            html = html.Replace("{{headerText}}", "Hello P135");


           _emailService.Send(newUser.Email, subject, html);
            
            //await _signInManager.SignInAsync(newUser, false);
            return RedirectToAction(nameof(VerifyEmail));

        }



        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return BadRequest();

            AppUser user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            await _userManager.ConfirmEmailAsync(user, token);

           
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");

        }

        public IActionResult VerifyEmail()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AppUser user = await _userManager.FindByEmailAsync(model.EmailOrUserName);

            if (user == null)
            {
                user=await _userManager.FindByNameAsync(model.EmailOrUserName);
            }


            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Email of Password is wrong");
                return View(model);
            }


            var result = await _signInManager.PasswordSignInAsync(user, model.PassWord, model.IsCheckUp, false);

            return RedirectToAction("Index", "Home");
        }
    }
}
