using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SDClubs.Models;

namespace SDClubs.Controllers
{
    public class SDRoleController : Controller
    {
        //private readonly ClubsContext _context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        //public SDRoleController(ClubsContext context)
        //{
        //    _context = context;

        //}

        public SDRoleController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        

        public IActionResult Index()
        {
            var roles = roleManager.Roles.OrderBy(a => a.Name);

            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            var role = roleManager.Roles.Where(a => a.Name == roleName).FirstOrDefault();

            if(role != null)
            {
                TempData["Message"] = "That role already exists!";
            }
            else if(string.IsNullOrEmpty(roleName))
            {
                TempData["Message"] = "You cannot add an empty name.";
            }
            else
            {
                roleName = roleName.Trim();
                TempData["Message"] = $"{roleName} created!";
                IdentityResult identityResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }    
            //var role = roleManager.Roles.Where(a => a.Name == roleName).FirstOrDefault();

            //var roles = roleManager.Roles.OrderBy(a => a.Name);

            return RedirectToAction("Index", "SDRole");
        }

        public async Task<IActionResult> DeleteRole(string name)
        {
            HttpContext.Session.SetString("role", name);
            if(name == "administrators")
            {
                TempData["Message"] = "You cannot delete the administrators role.";
                return RedirectToAction("Index", "SDRole");
            }
            var usersInRole = await userManager.GetUsersInRoleAsync(HttpContext.Session.GetString("role"));
            if(usersInRole.Count == 0)
            {
                return await Delete();
            }
            return View(usersInRole.OrderBy(a => a.UserName));
        }

        public async Task<IActionResult> Delete()
        {
            string name = HttpContext.Session.GetString("role");
            var role = await roleManager.Roles.Where(a => a.Name == name).FirstOrDefaultAsync();
            IdentityRole roleToDelete = role;

            await roleManager.DeleteAsync(roleToDelete);
            var roles = roleManager.Roles.OrderBy(a => a.Name);
            return RedirectToAction("Index", "SDRole");
        }

        public async Task<IActionResult> UserIndex(string roleName)
        {
            HttpContext.Session.SetString("role", roleName);
            var users = await userManager.Users.ToListAsync();
            var usersInRole = await userManager.GetUsersInRoleAsync(roleName);

            var usersDuplicate = await userManager.Users.ToListAsync();
            foreach (var user in usersDuplicate)
            {
                if(usersInRole.Contains(user))
                {
                    users.Remove(user);
                }
            }

            ViewData["UsersNotInRole"] = new SelectList(users.OrderBy(a=>a.UserName), "Id", "Email");

            return View(usersInRole.OrderBy(a=>a.UserName));
        }

        public async Task<IActionResult> RemoveUser(string id)
        {
            if(!string.IsNullOrEmpty(id))
            {
                var user = await userManager.Users.Where(a => a.Id == id).FirstOrDefaultAsync();
                if(User.Identity.Name == user.UserName)
                {
                    TempData["Message"] = "Bruh, no deleting yourself!";
                    return RedirectToAction("UserIndex", "SDRole", new { roleName = HttpContext.Session.GetString("role") });
                }

                await userManager.RemoveFromRoleAsync(user, HttpContext.Session.GetString("role"));
                TempData["Message"] = $"{user.UserName} successfully removed.";
            }
            else
            {
                TempData["Message"] = "Honestly I'm impressed you got here. But no.";
            }    
            return RedirectToAction("UserIndex", "SDRole", new {roleName = HttpContext.Session.GetString("role") });
        }

        public async Task<IActionResult> AddUser(string userId)
        {
            if(!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.Users.Where(a => a.Id == userId).FirstOrDefaultAsync();
                await userManager.AddToRoleAsync(user, HttpContext.Session.GetString("role"));
                TempData["Message"] = $"{user.UserName} successfully added.";
            }
            else
            {
                TempData["Message"] = "Ain't no one left to add!";
            }
            return RedirectToAction("UserIndex", "SDRole", new { roleName = HttpContext.Session.GetString("role") });
        }
    }
}