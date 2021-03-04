using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SDClubs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SDClubs.Controllers
{
    public class SDGroupMemberController : Controller
    {
        private readonly ClubsContext _context;

        public SDGroupMemberController(ClubsContext context)
        {
            _context = context;
        }

        // GET: SDGroupMember
        public async Task<IActionResult> Index(int? id, string firstName, string lastName, int? group, string groupName)
        {
            if(group != null) // certain spots I toss back group instead of ID. Why? Because I'm dumb, that's why. 
                              // But by the time I realized, I had used it extensively enough that fixing it was a lot of work.
                              // and it's late, and I have a midterm tomorrow. So yeah. 
            {
                id = group;
            }
            if (id != null)
            {
                HttpContext.Session.SetString("artistId", id.ToString());
            }
            else if (HttpContext.Session.GetString("artistId") != null)
            {
                id = int.Parse(HttpContext.Session.GetString("artistId"));
            }
            else
            {
                TempData["Message"] = "Select an artist";
                return RedirectToAction("Index", "SDArtist");
            }

            var artistContext = await _context.GroupMember.Include(g => g.ArtistIdGroupNavigation).Include(g => g.ArtistIdMemberNavigation.NameAddress).ToListAsync();
            var artistReturn = await _context.GroupMember.ToListAsync();
            string name;

            // if they passed a name, set it to our session variable
            if(firstName != null && lastName != null)
            {
                name = $"{lastName}, {firstName}";
            }
            else
            {
                name = $"{lastName}{firstName}";
            }

            HttpContext.Session.SetString("name", name);

            // if they came from a place that didn't specify name, it'll still pull the name from the selected record - sort of makes the above if statement
            // moot, but I wanted to demonstrate that I had done it the way the spec specified as well.
            if(groupName == "true" || HttpContext.Session.GetString("name") == "" || HttpContext.Session.GetString("name") == null)
            {
                var artistName = await _context.NameAddress.Where(a => a.NameAddressId == id).FirstOrDefaultAsync();
                string artistNameString;
                if(artistName.FirstName != null && artistName.LastName != null)
                {
                    artistNameString = $"{artistName.LastName}, {artistName.FirstName}";
                }
                else
                {
                    artistNameString = $"{artistName.LastName}{artistName.FirstName}";
                }
                HttpContext.Session.SetString("name", artistNameString);
            }

            // checks where to go by checking whether the group is in the member or group list
            foreach (var club in artistContext)
            {
                if (club.ArtistIdGroup == id) // if they were found in the groups section, keep them here
                {
                    artistReturn = await _context.GroupMember.Where(a => a.ArtistIdGroup == id).Include(g => g.ArtistIdGroupNavigation.NameAddress).OrderBy(a=>a.DateLeft).ThenBy(a=>a.DateJoined).ToListAsync();
                    return View(artistReturn);
                }
                else if (club.ArtistIdMember == id) // if they were found in the member section, send them to the new view we created, GroupsForArtist
                {
                    artistReturn = await _context.GroupMember.Where(a => a.ArtistIdMember == id).Include(g => g.ArtistIdGroupNavigation.NameAddress).OrderBy(a => a.DateLeft).ThenBy(a => a.DateJoined).ToListAsync();
                    TempData["Message"] = "This artist is an individual, so here are the groups they have been a part of:";
                    return View("GroupsForArtist", artistReturn);
                }
            }
            // if they weren't found in groups or artist, let the user know what's happening, and redirect them to create
            TempData["Message"] = "The selected artist is not part of a group, or a group themselves. You can add members to turn them into a group here!";
            return RedirectToAction("Create", "SDGroupMember");
            

        }

        // GET: SDGroupMember/Details/5
        public async Task<IActionResult> Details(int group, int member)
        {
            var groupMember = await _context.GroupMember.Include(a => a.ArtistIdGroupNavigation.NameAddress).Where(a => a.ArtistIdGroup == group).Where(a => a.ArtistIdMember == member).FirstOrDefaultAsync();
            if (groupMember == null)
            {
                return NotFound();
            }

            // sends the group name
            var name = await _context.NameAddress.Where(a => a.NameAddressId == group).FirstOrDefaultAsync();
            string groupNameString;
            if (name.FirstName != null && name.LastName != null)
            {
                groupNameString = $"{name.LastName}, {name.FirstName}";
            }
            else
            {
                groupNameString = $"{name.LastName}{name.FirstName}";
            }
            ViewBag.name = groupNameString;

            // sends the member name
            var memberName = await _context.NameAddress.Where(a => a.NameAddressId == member).FirstOrDefaultAsync();
            string artistNameString;
            if (memberName.FirstName != null && memberName.LastName != null)
            {
                artistNameString = $"{memberName.LastName}, {memberName.FirstName}";
            }
            else
            {
                artistNameString = $"{memberName.LastName}{memberName.FirstName}";
            }
            ViewBag.memName = artistNameString;


            return View(groupMember);
        }

        // GET: SDGroupMember/Create
        public IActionResult Create()
        {
            var artists = _context.Artist.ToList();
            var artistsDup = _context.Artist.Include(a => a.NameAddress).ToList();

            // this whole foreach is to remove items from the artists list if they are a group, or are currently in a group.
            foreach (var artist in artists)
            {
                var groups = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                var groupsDup = _context.GroupMember.Where(g => g.ArtistIdMember == artist.ArtistId).FirstOrDefault();

                if (groups != null) // if they are a group, poof
                {
                    artistsDup.Remove(artist);
                }
                else if (groupsDup != null) // if they are an artist, 
                {
                    if (groupsDup.DateLeft == null) // and they haven't left the group, poof
                    {
                        artistsDup.Remove(artist);
                    }
                }
            }

            // I used a couple ways to get the name across. I think this is the best. Definitely should have used this instead of my 
            // if statements in HTML, but hey, live and learn. It all works, so you get to be part of my growing process. 
            foreach (var artist in artistsDup)
            {
                string name = $"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}";
                artist.NameAddress.FirstName = name;
            }

            // we never end up using the top one here properly, but I wanted to leave it in case we fixed it up and used it later. 
            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "NameAddress.FirstName");
            ViewData["ArtistIdMember"] = new SelectList(artistsDup, "ArtistId", "NameAddress.FirstName");
            return View();
        }

        // POST: SDGroupMember/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            // sets the group id to the active group
            groupMember.ArtistIdGroup = int.Parse(HttpContext.Session.GetString("artistId"));
            
            // I asked about this stuff in discord and never got an answer, so I decided I'd just interpret it as wanting us to change the dates 
            // on the back end. I hope that's what you wanted. 
            DateTime today = DateTime.Now;
            groupMember.DateJoined = today; // sets the date joined to today no matter what they picked
            groupMember.DateLeft = null; // sets date joined to null no matter what they picked
            if (ModelState.IsValid)
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var artists = _context.Artist.ToList();
            var artistsDup = _context.Artist.Include(a => a.NameAddress).ToList();

            // same as above
            foreach (var artist in artists)
            {
                var groups = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                var groupsDup = _context.GroupMember.Where(g => g.ArtistIdMember == artist.ArtistId).FirstOrDefault();

                if (groups != null)
                {
                    artistsDup.Remove(artist);
                }
                else if (groupsDup != null)
                {
                    if (groupsDup.DateLeft == null)
                    {
                        artistsDup.Remove(artist);
                    }
                }
            }

            foreach (var artist in artistsDup)
            {
                string name = $"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}";
                artist.NameAddress.FirstName = name;
            }

            TempData["Message"] = "Something went wrong, please try again";

            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "NameAddress.FirstName");
            ViewData["ArtistIdMember"] = new SelectList(artistsDup, "ArtistId", "NameAddress.FirstName");
            return View(groupMember);
        }

        // GET: SDGroupMember/Edit/5
        public async Task<IActionResult> Edit(int group, int member)
        {
            var members = await _context.GroupMember.ToListAsync();

            var output = await _context.GroupMember.Where(a => a.ArtistIdGroup == group).Where(a => a.ArtistIdMember == member).FirstOrDefaultAsync();

            if (output == null)
            {
                return NotFound();
            }
            var name = await _context.NameAddress.Where(a => a.NameAddressId == group).FirstOrDefaultAsync();
            string artistNameString;
            if (name.FirstName != null && name.LastName != null)
            {
                artistNameString = $"{name.LastName}, {name.FirstName}";
            }
            else
            {
                artistNameString = $"{name.LastName}{name.FirstName}";
            }
            ViewBag.name = artistNameString;

            ViewData["ArtistIdGroup"] = new SelectList(members, "ArtistIdGroup", "ArtistIdGroup");
            ViewData["ArtistIdMember"] = new SelectList(members, "ArtistIdMember", "ArtistIdMember"/*, groupMember.ArtistIdMember*/);
            return View(output);
        }

        // POST: SDGroupMember/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.ArtistIdGroup))
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
            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdGroup);
            ViewData["ArtistIdMember"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdMember);
            return View(groupMember);
        }

        // GET: SDGroupMember/Delete/5
        public async Task<IActionResult> Delete(int group, int member)
        {
            var groupMember = await _context.GroupMember.Include(a=>a.ArtistIdGroupNavigation.NameAddress).Where(a => a.ArtistIdGroup == group).Where(a => a.ArtistIdMember == member).FirstOrDefaultAsync();
            if (groupMember == null)
            {
                return NotFound();
            }

            // sends the group name
            var name = await _context.NameAddress.Where(a => a.NameAddressId == group).FirstOrDefaultAsync();
            string groupNameString;
            if (name.FirstName != null && name.LastName != null)
            {
                groupNameString = $"{name.LastName}, {name.FirstName}";
            }
            else
            {
                groupNameString = $"{name.LastName}{name.FirstName}";
            }
            ViewBag.name = groupNameString;

            // sends the member name
            var memberName = await _context.NameAddress.Where(a => a.NameAddressId == member).FirstOrDefaultAsync();
            string artistNameString;
            if (memberName.FirstName != null && memberName.LastName != null)
            {
                artistNameString = $"{memberName.LastName}, {memberName.FirstName}";
            }
            else
            {
                artistNameString = $"{memberName.LastName}{memberName.FirstName}";
            }
            ViewBag.memName = artistNameString;

            // sends member and group ids
            ViewBag.member = member;
            ViewBag.group = group;
            return View(groupMember);
        }

        // POST: SDGroupMember/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int member, int group)
        {
            var groupMember = await _context.GroupMember.Include(a => a.ArtistIdGroupNavigation.NameAddress).Where(a => a.ArtistIdMember == member).Where(a => a.ArtistIdGroup == group).FirstOrDefaultAsync();
            _context.GroupMember.Remove(groupMember);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMember.Any(e => e.ArtistIdGroup == id);
        }
    }
}
