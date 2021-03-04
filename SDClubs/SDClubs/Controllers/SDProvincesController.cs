/* Name: Stephen Draper
 * Student Number: 8616452
 * October 9, 2020
 * sdraper6452@conestogac.on.ca
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SDClubs.Models;

namespace SDClubs.Controllers
{
    public class SDProvincesController : Controller
    {
        private readonly ClubsContext _context;

        public SDProvincesController(ClubsContext context)
        {
            _context = context;
        }

        // GET: SDProvinces
        public async Task<IActionResult> Index(string code, string name)
        {
            if(code == null || code == "") // check if anything was passed through the country code, if not...
            {
                if (Request.Cookies["Cookie_Data"] == null || Request.Cookies["Cookie_Data"] == "Hello Cookie" || Request.Cookies["Cookie_Data"] == "") // check if anything was passed via cookies
                                                            // APPARENTLY COOKIES DEFAULT TO HELLO COOKIE?!? My code kept messing up until I assigned it and saw it was assigning itself that value
                {
                    if (HttpContext.Session.Get("Session_Code") == null || HttpContext.Session.Get("Session_Code").ToString() == "") // check if anything was passed via session variable
                    {
                        TempData["Message"] = "Please select a country code to go to associated provinces."; // if not, set temp data for our layout view to catch
                        //return View("/Views/SDCountries/Index.cshtml",await _context.Country.ToListAsync()); // send the user back to countries
                        return RedirectToAction("Index", "SDCountries");
                        //Response.Redirect("SDCountries/Index");        This was interesting, because it lost our tempdata. Had to change it to view to retain it.
                    }
                    else
                    {
                        code = HttpContext.Session.GetString("Session_Code"); // if a session variable was found, set the code to the session variable
                    }
                }
                else
                {
                    code = Request.Cookies["Cookie_Data"]; // if a cookie was found, set the code to the cookie (this will never happen in our program)
                }
            }
            else
            {
                HttpContext.Session.SetString("Session_Code", code); // if code was found, set the session variable to our country code
            }

            if(name != null && name != "")
            {
                HttpContext.Session.SetString("Session_Name", name); // if they passed a name, set it to 
            }
            else // if they didn't pass a name
            {
                var existingName = _context.Country.Where(p => p.CountryCode == code).FirstOrDefault().Name; // pull country name 
                HttpContext.Session.SetString("Session_Name", existingName); // set
            }

            var clubsContext = _context.Province.Where(p => p.CountryCode == code).OrderBy(c=>c.Name).ToListAsync(); // pulls elements that match country code and sorts by name
            
            return View(await clubsContext);
        }

        // GET: SDProvinces/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // GET: SDProvinces/Create
        public IActionResult Create()
        {
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode");
            return View();
        }

        // POST: SDProvinces/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            var existingProvince = _context.Province.Where(p => p.ProvinceCode == province.ProvinceCode).FirstOrDefault(); // checks the database if there's an existing province with the same code
            var existingName = _context.Province.Where(p => p.Name == province.Name).FirstOrDefault();
            string errorMessage ="";

            // NOTE: I'm aware that the database checks against ALL provinces, not just the ones for the selected country. However, we cannot change this
            // as it is using the province code as a primary key, so if we were to duplicate it, it would throw an exception.

            if (ModelState.IsValid)
            {
                if (existingProvince != null) // if it didn't find anything, keep going
                {
                    errorMessage += $"The province code {existingProvince.ProvinceCode} already exists.";
                    //return RedirectToAction(nameof(Index));
                }
                if(existingName != null)
                {
                    errorMessage += "The province name " + existingName.Name + " already exists.";
                }
                else
                {
                    _context.Add(province);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["Message"] = errorMessage;
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // GET: SDProvinces/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // POST: SDProvinces/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            var existingName = _context.Province.Where(p => p.Name == province.Name).FirstOrDefault(); // pulls 

            if(existingName != null)
            {
                TempData["Message"] = "That province name already exists.";
                return View(province);
            }

            if (id != province.ProvinceCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(province);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvinceExists(province.ProvinceCode))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // GET: SDProvinces/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // POST: SDProvinces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var province = await _context.Province.FindAsync(id);
            _context.Province.Remove(province);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvinceExists(string id)
        {
            return _context.Province.Any(e => e.ProvinceCode == id);
        }
    }
}
