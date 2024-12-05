using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShiftLogger.Models;
using System.Threading.Tasks;

namespace ShiftLogger.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly ShiftLoggerContext _context;
        private readonly UserManager<User> _userManager;

        public ShiftsController(ShiftLoggerContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays shifts based on user role
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            // Build the query for fetching shifts
            IQueryable<Shift> shiftsQuery = _context.Shifts
                .Include(c => c.Car)
                .Include(d => d.Driver);

            // Filter shifts based on user role: admins can see all shifts, others only their own
            if (!isAdmin)
            {
                shiftsQuery = shiftsQuery.Where(s => s.Driver.Id == userId);
            }

            // Fetch the shifts and return to the view
            var shifts = await shiftsQuery.ToListAsync();
            return View(shifts);
        }

        // Displays the form for creating a new shift
        [HttpGet]
        public async Task<IActionResult> CreateShift()
        {
            // Populate dropdowns with users and cars for shift creation
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker");
            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            // Initialize the shift with the current date
            Shift shift = new Shift { Date = DateTime.Now };
            return View(shift);
        }

        // Handles the form submission to create a new shift
        [HttpPost]
        public async Task<IActionResult> CreateShift(Shift shift)
        {
            // Ensure that the selected car and driver exist in the database
            shift.Car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == shift.Car.Id);
            shift.Driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == shift.Driver.Id);

            // Remove any validation errors related to the Car and Driver properties
            ModelState.RemoveAll<Shift>(x => x.Driver != null || x.Car != null);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model is not valid. Errors:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                // Repopulate the dropdowns and return the form with the current shift data
                ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName", shift.Driver?.Id);
                ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Name", shift.Car?.Id);
                return View(shift); 
            }

            // If validation passes, add the new shift and redirect
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Displays the form for editing an existing shift
        [HttpGet]
        public async Task<IActionResult> EditShift(int id)
        {
            // Fetch the shift to be edited along with related entities
            var shift = await _context.Shifts
                .Include(s => s.Driver)
                .Include(s => s.Car)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return NotFound();

            // Populate dropdowns for users and cars
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker");

            return View(shift);
        }

        // Handles the form submission for editing an existing shift
        [HttpPost]
        public async Task<IActionResult> EditShift(Shift shift)
        {
            // Ensure that the selected car and driver exist in the database
            shift.Car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == shift.Car.Id);
            shift.Driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == shift.Driver.Id);

            // Remove any validation errors related to the Car and Driver properties
            ModelState.RemoveAll<Shift>(x => x.Driver != null || x.Car != null);

            if (ModelState.IsValid)
            {
                // Fetch the existing shift from the database for updating
                var existingShift = await _context.Shifts
                    .Include(s => s.Driver)
                    .Include(s => s.Car)
                    .FirstOrDefaultAsync(s => s.Id == shift.Id);

                if (existingShift == null)
                    return NotFound();

                // Update the properties of the existing shift
                existingShift.Driver = shift.Driver;
                existingShift.Car = shift.Car;
                existingShift.Date = shift.Date;
                existingShift.TaxiPort = shift.TaxiPort;
                existingShift.Liftago = shift.Liftago;
                existingShift.Bolt = shift.Bolt;
                existingShift.Other = shift.Other;
                existingShift.Distance = shift.Distance;

                try
                {
                    // Save the updated shift data
                    _context.Update(existingShift);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    AddErrorsToModelState("Nastala chyba při ukládání směny. Zkuste znovu");
                }
            }

            // Repopulate the dropdowns and return the form with errors
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName", shift.Driver?.Id);
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker", shift.Car?.Id);
            return View(shift);
        }

        // Handle shift deletion
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // Centralized error handling method for adding errors to ModelState
        private void AddErrorsToModelState(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
