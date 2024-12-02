using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ShiftLogger.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ShiftLogger.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UserManagementController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }


        private async Task<bool> UpdateUserDetailsAsync(User user, User model)
        {
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.MustChangePassword = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, ErrorMsgCz(error.Code));
                }
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove roles.");
            }

            result = await _userManager.AddToRoleAsync(user, model.Role.ToString());
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to assign role.");
            }

            if (!string.IsNullOrEmpty(model.Pass))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Pass);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, ErrorMsgCz(error.Code));
                    }
                    return false;
                }
            }

            return true;
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        { 
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            User model = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role

            };

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                if (await UpdateUserDetailsAsync(user, model))
                    return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            User model = new();
            model = user;

            ViewBag.changePass = user.MustChangePassword;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MyAccount(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                if (await UpdateUserDetailsAsync(user, model))
                {
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Pass))
                {
                    ModelState.AddModelError("Pass", "Heslo je vyžadováno.");
                }
                else
                {
                    User user = new()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Role = model.Role,
                        MustChangePassword = true
                    };


                    var result = await _userManager.CreateAsync(user, model.Pass);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }

                    await _userManager.AddToRoleAsync(user, model.Role.ToString());

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, ErrorMsgCz(error.Code));
                    }
                }
            }

            model.Pass = model.Pass;

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        private string ErrorMsgCz(string erCode)
        {
            if (erCode == "PasswordTooShort")
                return new string($"Heslo musí mít alespoň {_userManager.Options.Password.RequiredLength} znaků");
            else if (erCode == "PasswordRequiresDigit")
                return new string("Heslo musí mít alespoň jednu číslici ('0' - '9')");
            else if (erCode == "PasswordRequiresUpper")
                return new string("Heslo musí mít alespoň jedno velké písmeno ('A' - 'Z')");
            else if (erCode == "PasswordRequiresLower")
                return new string("Heslo musí mít alespoň jedno malé písmeno ('A' - 'Z')");
            else if (erCode == "DuplicateUserName")
                return new string("Jméno již existuje");


            else
                return new string("Neznámá chyba");
        }
    }
}