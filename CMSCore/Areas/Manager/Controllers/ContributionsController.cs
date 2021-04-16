using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMSCore.Data;
using CMSCore.Models;
using System.Security.Claims;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Aspose.Words;

namespace CMSCore.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class ContributionsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public ContributionsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }

        // GET: Cordinator/Contributions
        public async Task<IActionResult> Index(int topicId)
        {
            var contributions = await _context.Contributions.Include(c => c.Topic)
                                                           .Include(c => c.Contributor)
                                                           .Where(c => c.TopicId == topicId)
                                                           .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleId = await _context.UserRoles.Where(u => u.UserId == userId)
                                           .Select(u => u.RoleId).FirstOrDefaultAsync();

            if (contributions != null)
            {
                if (roleId == "Manager")
                {
                    contributions = contributions.Where(c => c.Status == ContributionStatus.Approved).ToList();
                }
                else if (roleId == "Cordinator")
                {
                    var user = await _context.Users.FindAsync(userId);
                    contributions = contributions.Where(c => c.Contributor.DepartmentId == user.DepartmentId).ToList();
                }
            }
            ViewData["TopicId"] = topicId;

            return View(contributions);
        }

        // GET: Cordinator/Contributions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contribution = await _context.Contributions
                .Include(c => c.Contributor)
                .Include(c => c.Topic)
                .Include(c => c.SubmittedFiles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contribution == null)
            {
                return NotFound();
            }
            var comments = await _context.Comments.Include(c => c.User)
                .Where(c => c.ContributionId == id)
                .OrderBy(c => c.Date)
                .ToListAsync();
            foreach (var file in contribution.SubmittedFiles)
            {
                // Load the document from disk.
                Document doc = new Document(file.URL);
                // Save as PDF
                var path = Path.Combine(_env.WebRootPath, _Global.PATH_TEMP, Path.GetFileNameWithoutExtension(file.URL) + ".pdf");
                doc.Save(path);
            }

            ViewData["Comments"] = comments;

            return View(contribution);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int contributionId, string commentContent)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                var existContribution = await _context.Contributions.FindAsync(contributionId);

                if (existContribution != null && !String.IsNullOrEmpty(commentContent))
                {
                    var comment = new Models.Comment();
                    comment.Content = commentContent;
                    comment.Date = DateTime.Now;
                    comment.ContributionId = existContribution.Id;
                    comment.UserId = userId;
                    _context.Add(comment);
                    await _context.SaveChangesAsync();

                }

                //    return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Details), new { id = contributionId });
        }
        public async Task<ActionResult> DownloadApprovedFile(int topicId = -1)
        {
            var approvedContributions = await _context.Contributions.Include(c => c.Contributor)
                                                                    .Include(c => c.SubmittedFiles)
                                                                    .Where(c => c.TopicId == topicId
                                                                    && c.Status == ContributionStatus.Approved).ToListAsync();

            if (approvedContributions.Count() > 0)
            {
                var topic = await _context.Topics.FindAsync(topicId);
                var zipPath = Path.Combine(_env.WebRootPath, _Global.PATH_TOPIC, topicId.ToString(), topic.Title + ".zip");

                using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive achive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (var contribution in approvedContributions)
                        {
                            foreach (var file in contribution.SubmittedFiles)
                            {
                                achive.CreateEntryFromFile(file.URL, Path.Combine(contribution.Contributor.UserNumber
                                                                                    , file.URL.Split("\\").Last()));
                            }
                        }
                    }
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(zipPath);

                System.IO.File.Delete(zipPath);


                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Zip, zipPath.Split("\\").Last());
            }

            return NoContent();
        }
        public async Task<ActionResult> DownloadFile(int fileId = -1)
        {
            var file = await _context.SubmittedFiles.FindAsync(fileId);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file.URL);
            return File(fileBytes, MediaTypeNames.Application.Octet, Path.GetFileName(file.URL));
        }
    }
}
