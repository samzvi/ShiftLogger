using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftLogger.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ShiftLogger.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly ShiftLoggerContext _context;
        private readonly UserManager<User> _userManager;

        public UserManagementController(ShiftLoggerContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the list of users for admins
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // Asynchronous method to update user details
        private async Task<bool> UpdateUserDetailsAsync(User user, User model)
        {
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.MustChangePassword = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                AddErrorsToModelState(result);
                return false;
            }

            // Remove current roles and assign the new role
            var currentRoles = await _userManager.GetRolesAsync(user);
            var roleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove roles.");
                return false;
            }
            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role.ToString());
            if (!addRoleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to assign role.");
                return false;
            }

            // Reset the password if a new password is provided
            if (!string.IsNullOrEmpty(model.Pass))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Pass);
                if (!passwordResult.Succeeded)
                {
                    AddErrorsToModelState(passwordResult);
                    return false;
                }
            }

            return true;
        }

        // Displays the form to edit an existing user
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            // Create a model for the edit form
            User model = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        // Handles the form submission for editing a user
        [HttpPost]
        public async Task<IActionResult> EditUser(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                // Update the user details using the UpdateUserDetailsAsync method
                if (await UpdateUserDetailsAsync(user, model))
                    return RedirectToAction("Index");
            }

            return View(model);
        }

        // Displays the current user's account information
        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Create a model with the current user's details
            User model = new()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            // Flag for view to indicate is pass change is required
            ViewBag.changePass = user.MustChangePassword;

            return View(model);
        }

        // Handles the form submission to update the current user's account information
        [HttpPost]
        public async Task<IActionResult> MyAccount(User model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                // Update the user's details using the UpdateUserDetailsAsync method
                if (await UpdateUserDetailsAsync(user, model))
                {
                    // Redirect to different pages based on user role
                    if (User.IsInRole("Admin"))
                        return RedirectToAction("Index");
                    return RedirectToAction("Index", "Shifts");
                }
            }

            return View(model);
        }

        // Displays the form to create a new user (admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        // Handles the form submission for creating a new user
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
                    // Create a new user with the provided data
                    User user = new()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Role = model.Role,
                        MustChangePassword = true
                    };

                    // Create the user and add them to the selected role
                    var result = await _userManager.CreateAsync(user, model.Pass);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.Role.ToString());
                        return RedirectToAction("Index");
                    }

                    // Add errors to ModelState if creation fails
                    AddErrorsToModelState(result);
                }
            }

            return View(model);
        }

        // Handles the deletion of a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(); 
            }

            // Get all shifts related to the user (if any)
            var userShifts = _context.Shifts.Where(s => s.Driver == user).ToList();

            // If there are shifts associated with the user, set DriverId to null
            if (userShifts.Any())
            {
                foreach (var shift in userShifts)
                {
                    shift.Driver = null;
                }
                await _context.SaveChangesAsync();
            }

            // Proceed with deleting the user after handling shifts
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            AddErrorsToModelState(result);
            return RedirectToAction(nameof(Index));
        }


        // Centralized method to add errors from IdentityResult to ModelState
        private void AddErrorsToModelState(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, ErrorMsgCz(error.Code)); 
            }
        }

        // Method to translate error codes into messages in Czech
        private string ErrorMsgCz(string erCode)
        {
            switch (erCode)
            {
                case "PasswordTooShort":
                    return $"Heslo musí mít alespoň {_userManager.Options.Password.RequiredLength} znaků";
                case "PasswordRequiresDigit":
                    return "Heslo musí mít alespoň jednu číslici ('0' - '9')";
                case "PasswordRequiresUpper":
                    return "Heslo musí mít alespoň jedno velké písmeno ('A' - 'Z')";
                case "PasswordRequiresLower":
                    return "Heslo musí mít alespoň jedno malé písmeno ('a' - 'z')";
                case "DuplicateUserName":
                    return "Jméno již existuje";
                case "InvalidUserName":
                    return "Jméno obsahuje nedovolené znaky";
                default:
                    return "Neznámá chyba";
            }
        }
    }
}
