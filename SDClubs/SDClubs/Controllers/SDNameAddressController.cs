using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SDClubs.Models;

namespace SDClubs.Controllers
{
    [Authorize(Roles = "Admin")] // login required
    public class SDNameAddressController : Controller
    {
        private readonly ClubsContext _context;

        public SDNameAddressController(ClubsContext context)
        {
            _context = context;
        }

        // GET: SDNameAddress
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var nameContext = _context.NameAddress.Include(n => n.ProvinceCodeNavigation);

            foreach (var name in nameContext)
            {
                string tempName;
                if (name.FirstName != null && name.LastName != null && name.FirstName != "" && name.LastName != "")
                {
                    tempName = $"{name.LastName}, {name.FirstName}";
                }
                else
                {
                    tempName = $"{name.LastName}{name.FirstName}";
                }
                name.FirstName = tempName;
            }


            return View(await nameContext.ToListAsync());
        }

        // GET: SDNameAddress/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);
            if (nameAddress == null)
            {
                return NotFound();
            }

            return View(nameAddress);
        }

        // GET: SDNameAddress/Create
        public IActionResult Create()
        {
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode");
            return View();
        }

        // POST: SDNameAddress/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nameAddress);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Create successful.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        // GET: SDNameAddress/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress.FindAsync(id);
            if (nameAddress == null)
            {
                return NotFound();
            }
            var provinceName = _context.Province.Where(a => a.ProvinceCode == nameAddress.ProvinceCode).FirstOrDefault();
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(a=>a.Name), "ProvinceCode", "Name");
            return View(nameAddress);
        }

        // POST: SDNameAddress/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            if (id != nameAddress.NameAddressId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nameAddress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NameAddressExists(nameAddress.NameAddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Message"] = "Update successful.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(a=>a.Name), "Name", "Name");
            return View(nameAddress);
        }

        // GET: SDNameAddress/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);
            if (nameAddress == null)
            {
                return NotFound();
            }
            return View(nameAddress);
        }

        // POST: SDNameAddress/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nameAddress = await _context.NameAddress.FindAsync(id);
            _context.NameAddress.Remove(nameAddress);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Delete successful.";
            return RedirectToAction(nameof(Index));
        }

        private bool NameAddressExists(int id)
        {
            return _context.NameAddress.Any(e => e.NameAddressId == id);
        }
    }
}
