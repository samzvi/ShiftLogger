using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShiftLogger.Models;

namespace ShiftLogger.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public HomeController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Action for showing the home page or redirecting to UserManagement
        [HttpGet]
        public IActionResult Index()
        {
            // Redirect authenticated users to UserManagement page
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "UserManagement");
                return RedirectToAction("Index", "Shifts");

            }

            // Return the login view for unauthenticated users
            return View(new LoginViewModel());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Action for handling the login process
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if user exists by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // User not found, add error to ModelState
                    AddErrorsToModelState("Email neexistuje");
                    return View("Index", model);
                }

                // Attempt to sign the user in
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    // Check if the user must change their password
                    if (user.MustChangePassword)
                    {
                        return RedirectToAction("MyAccount", "UserManagement");
                    }

                    // Redirect to Shifts Index page if login is successful
                    return RedirectToAction("Index", "Shifts");
                }
                else
                {
                    // Invalid password, add error to ModelState
                    AddErrorsToModelState("Nesprávné heslo");
                }
            }

            // Return to the same page if the login is invalid
            return View("Index", model);
        }

        // Action for handling logout process
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign the user out and redirect to the home page
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Centralized method to handle adding error messages to ModelState
        private void AddErrorsToModelState(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
