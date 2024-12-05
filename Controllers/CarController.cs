using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftLogger.Data;
using ShiftLogger.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ShiftLogger.Controllers
{
    public class CarController : Controller
    {
        private readonly ShiftLoggerContext _context;

        public CarController(ShiftLoggerContext context)
        {
            _context = context;
        }

        // Displays all cars (admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cars.ToListAsync());
        }


        // Displays the form to create a new car
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateCar()
        {
            return View();
        }

        // Handles the form submission to create a new car
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCar(Car car)
        {
            if (ModelState.IsValid)
            {
                // Add the new car to the context and save the changes
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }

        // Displays the form to edit an existing car
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            // Return the edit view with the car's data
            return View("EditCar", car);
        }

        // Handles the form submission to update an existing car
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car)
        {
            if (id != car.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingCar = await _context.Cars.FindAsync(id);
                if (existingCar == null)
                    return NotFound();

                // Update the car's properties with the submitted data
                existingCar.SPZ = car.SPZ;
                existingCar.Name = car.Name;
                existingCar.Marker = car.Marker;

                try
                {
                    // Save the changes to the database
                    _context.Update(existingCar);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    AddErrorsToModelState("Nastala chyba při ukládání auta. Zkuste znovu");
                }
            }

            return View("EditCar", car);
        }

        // Handles the car deletion process
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            // Remove the car from the context and save changes
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Centralized error handling method to add errors to ModelState
        private void AddErrorsToModelState(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
