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


        public async Task<IActionResult> Index()
        {
            return View(await _context.Cars.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }


        public IActionResult CreateCar()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCar([Bind("Id,SPZ,Name,Marker")] Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound();

            return View("EditCar", car);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SPZ,Name,Marker")] Car car)
        {
            if (id != car.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingCar = await _context.Cars.FindAsync(id);
                if (existingCar == null)
                {
                    return NotFound();
                }

                existingCar.SPZ = car.SPZ;
                existingCar.Name = car.Name;
                existingCar.Marker = car.Marker;

                try
                {
                    _context.Update(existingCar);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, "Nastala chyba při ukládání auta. Zkuste znovu");
                }
            }

            return View("EditCar", car);
        }



        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
