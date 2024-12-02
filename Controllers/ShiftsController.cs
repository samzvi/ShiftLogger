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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("Is user Admin?: " + User.IsInRole("Admin"));
            var shifts = await _context.Shifts
                .Include(c => c.Car)
                .Include(d => d.Driver)
                .ToListAsync();
            return View(shifts);
        }

        [HttpGet]
        public async Task<IActionResult> CreateShift()
        {
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker");
            ViewBag.CurrentUserId = _userManager.GetUserId(User);

            Shift shift = new Shift();
            shift.Date = DateTime.Now;

            return View(shift);
        }


        [HttpPost]
        public async Task<IActionResult> CreateShift(Shift shift)
        {
            shift.Car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == shift.Car.Id);
            shift.Driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == shift.Driver.Id);

            ModelState.RemoveAll<Shift>(x => x.Driver != null || x.Car != null);

            if (ModelState.IsValid)
            {
                _context.Shifts.Add(shift);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("Model is not valid. Errors:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // Or log it somewhere else
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", shift.Driver?.Id);
            ViewBag.Cars = new SelectList(_context.Cars, "Id", "Name", shift.Car?.Id);
            return View(shift);
        }


        [HttpGet]
        public async Task<IActionResult> EditShift(int id)
        {
            // Fetch the shift to be edited
            var shift = await _context.Shifts
                .Include(s => s.Driver)
                .Include(s => s.Car)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();  // If the shift doesn't exist
            }

            // Populate ViewBag for users and cars
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker");

            return View(shift);
        }

        [HttpPost]
        public async Task<IActionResult> EditShift(Shift shift)
        {
            shift.Car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == shift.Car.Id);
            shift.Driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == shift.Driver.Id);

            ModelState.RemoveAll<Shift>(x => x.Driver != null || x.Car != null);

            if (ModelState.IsValid)
            {
                // Update the Shift details
                var existingShift = await _context.Shifts
                    .Include(s => s.Driver)
                    .Include(s => s.Car)
                    .FirstOrDefaultAsync(s => s.Id == shift.Id);

                if (existingShift == null)
                {
                    return NotFound();  // If the shift doesn't exist anymore
                }

                // Update the properties
                existingShift.Driver = shift.Driver;
                existingShift.Car = shift.Car;
                existingShift.Date = shift.Date;
                existingShift.TaxiPort = shift.TaxiPort;
                existingShift.Liftago = shift.Liftago;
                existingShift.Bolt = shift.Bolt;
                existingShift.Other = shift.Other;
                existingShift.Distance = shift.Distance;

                // Save the changes
                _context.Update(existingShift);
                await _context.SaveChangesAsync();

                // Redirect to the list or detail page
                return RedirectToAction(nameof(Index));
            }

            ////////////////////
            Console.WriteLine("Model is not valid. Errors:");
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage); // Log the error message
                }
            }

            // If ModelState is invalid, re-populate ViewBag and return the view
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName", shift.Driver?.Id);
            ViewBag.Cars = new SelectList(await _context.Cars.ToListAsync(), "Id", "Marker", shift.Car?.Id);

            return View(shift);
        }




    }
}
