﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMSCore.Data;
using CMSCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace CMSCore.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index(string SearchString, string sortOrder,int? pageNumber, string currentFilter)
        {
            ViewData["CurrentSort"] = sortOrder;

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["UserSortParm"] = String.IsNullOrEmpty(sortOrder) ? "User_desc" : "";
            ViewData["CurrentFilter"] = SearchString;
            if (SearchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            var user = from m in _context.Users
                       select m;
            if (!String.IsNullOrEmpty(SearchString))
            {
                user = user.Where(s => s.FirstName.Contains(SearchString)
                || s.LastName.Contains(SearchString) );

            }
            switch (sortOrder)
            {
                case "User_desc":
                    user = user.OrderByDescending(s => s.UserName);
                    break;
                case "name_desc":
            user = user.OrderByDescending(s => s.LastName);
            break;
               case "Date":
                    user = user.OrderBy(s => s.FirstName);
            break;
               case "date_desc":
                    user = user.OrderByDescending(s => s.FirstName);
            break;
                default:
                    user = user.OrderBy(s => s.LastName);
            break;
    }

            int pageSize = 5;
            return View(await PaginatedList<CUser>.CreateAsync(user.AsNoTracking(), pageNumber ?? 1, pageSize));


        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CUser user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    

        // GET: admin/user/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CUser user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: admin/User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: Admin/user/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CUser user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: admin/user/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            CUser user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public async Task<ActionResult> Assign(string Id)
        {
            var user = await _context.Users.FindAsync(Id);

            var UserRoleIds = _context.UserRoles.ToList()
                                          .Where(ur => ur.UserId == Id)
                                          .Select(ur => ur.RoleId);
            //created
            var currentRoles = _context.Roles.Where(r => UserRoleIds.Contains(r.Id)).ToList();
            //not existed
            var remainRoles = _context.Roles.ToList().Where(r1 => !currentRoles.Any(r2 => r2.Id == r1.Id));

            ViewData["currentRoles"] = currentRoles;
            ViewData["remainRoles"] = new SelectList(remainRoles, "Id", "Id");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToRole(string userId, string[] remainRoles)
        {
            CUser currentUser = _context.Users.Find(userId);

            if (remainRoles != null && remainRoles.Count() > 0)
            {
                foreach (string item in remainRoles)
                {
                    _context.UserRoles.Add(new IdentityUserRole<string>()
                    {
                        UserId = userId,
                        RoleId = item
                    });
                }
                _context.SaveChanges();
            }
            var UserRoleIds = _context.UserRoles.ToList()
                                          .Where(ur => ur.UserId == userId)
                                          .Select(ur => ur.RoleId);
            //created
            var currentRoles = _context.Roles.Where(r => UserRoleIds.Contains(r.Id)).ToList();
            //not existed
              

            ViewData["currentRoles"] = currentRoles;
            ViewData["remainRoles"] = new SelectList(_context.Roles.ToList().Where(r1 => !currentRoles.Any(r2 => r2.Id == r1.Id)), "Id", "Id");

            return RedirectToAction("Assign", new { Id = userId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRole(string userId, string roleId)
        {
            
            if (userId != null && roleId != null)
            {
                    _context.UserRoles.Remove(new IdentityUserRole<string>()
                    {
                        UserId = userId,
                        RoleId = roleId
                    });
             
                _context.SaveChanges();
            }
            var UserRoleIds = _context.UserRoles.ToList()
                                          .Where(ur => ur.UserId == userId)
                                          .Select(ur => ur.RoleId);
            //created
            var currentRoles = _context.Roles.Where(r => UserRoleIds.Contains(r.Id)).ToList();
            //not existed


            ViewData["currentRoles"] = currentRoles;
            ViewData["remainRoles"] = new SelectList(_context.Roles.ToList().Where(r1 => !currentRoles.Any(r2 => r2.Id == r1.Id)), "Id", "Id");

            return RedirectToAction("Assign", new { Id = userId });
        }


    }
}
