using EntityFrameWork.Models;
using EntityFrameWork.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EntityFrameWork.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            await _signInManager.SignInAsync(newUser, false);

            return RedirectToAction("Index", "Home");



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
