using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShiftLogger.Models;

namespace ShiftLogger.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public IActionResult AccessDenied()
        {
            return View();
        }

        public HomeController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "UserManagement");
            }
            return View(new LoginViewModel());
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        if (user.MustChangePassword)
                        {
                            return RedirectToAction("MyAccount", "UserManagement");
                        }

                        return RedirectToAction("Index", "Shifts");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nesprávné heslo");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email neexistuje");
                }
            }

            return View("Index", model);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
