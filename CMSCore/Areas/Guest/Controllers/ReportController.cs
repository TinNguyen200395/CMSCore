using CMSCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMSCore.Controllers
{
    [Area("Guest")]
    [Authorize(Roles = "Guest")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: ReportController
        public async Task<IActionResult> Index(int departmentId = -1)
        {
            var studentIds = await _context.UserRoles.Where(u => u.RoleId == "Student").Select(u => u.UserId).ToListAsync();
            var coordinatorIds = await _context.UserRoles.Where(c => c.RoleId == "Cordinator").Select(c => c.UserId).ToListAsync();
            var coordinators = await _context.Users.Where(c => coordinatorIds.Contains(c.Id)).ToListAsync();
            var students = await _context.Users.Where(u => studentIds.Contains(u.Id)).ToListAsync();
            var StudentOfDeparment = students.Where(s => s.DepartmentId == departmentId).ToList();
            if (departmentId != -1)
            { 
                students = students.Where(s => s.DepartmentId == departmentId).ToList();
            }
            ViewData["TotalStudent"] = students.Count();
            ViewData["TotalStudentOfDeparment"] = StudentOfDeparment.Count();
            ViewData["TotalDeparment"] = await _context.Departments.CountAsync();
            ViewData["TotalCoordinater"] = coordinators.Count();

            return View();
        }
    }
}
