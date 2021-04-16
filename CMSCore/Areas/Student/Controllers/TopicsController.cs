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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace CMSCore.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class TopicsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public TopicsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }

        // GET: Student/Topics
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var topicIds = await _context.Contributions.Where(c => c.ContributorId == userId)
                                                              .Select(c => c.TopicId)
                                                              .ToListAsync();
            var topics = await _context.Topics.Where(t => t.DeadLine_2 >= DateTime.Now || topicIds.Contains(t.Id))
                                              .OrderByDescending(t => t.DeadLine_2)
                                              .ToListAsync();
            return View(topics);
        }

        // GET: Student/Topics/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Contribution = await _context.Contributions.Include(c => c.SubmittedFiles)
                                                          .FirstOrDefaultAsync(c => c.ContributorId == userId && c.TopicId == id);
            List<Comment> comments = null;
            if (Contribution != null)
            {
                comments = await _context.Comments.Include(c => c.User).Where(c => c.ContributionId == Contribution.Id).OrderBy(c => c.Date).ToListAsync();
            }
            ViewData["Comments"] = comments;
            ViewData["ContributorId"] = userId;
            ViewData["Contributions"] = Contribution;
            return View(topic);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Contribution contribution, IFormFile file)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == contribution.TopicId);
            if (topic.DeadLine_2 >= DateTime.Now)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (ModelState.IsValid)
                {
                    var user = await _context.Users.FindAsync(userId);
                    var existContribution = await _context.Contributions.FirstOrDefaultAsync(c => c.ContributorId == userId && c.TopicId == contribution.TopicId);

                    if (existContribution == null)
                    {
                        contribution.ContributorId = userId;
                        contribution.Status = ContributionStatus.Pending;

                        _context.Add(contribution);
                        await _context.SaveChangesAsync();

                        existContribution = contribution;
                    }

                    else
                    {
                        existContribution.Status = ContributionStatus.Pending;

                        _context.Update(existContribution);
                        await _context.SaveChangesAsync();
                    }

                    if (file.Length > 0)
                    {
                        FileType? fileType;
                        string fileExtension = Path.GetExtension(file.FileName).ToLower();

                        switch (fileExtension)
                        {
                            case ".doc": case ".docx": fileType = FileType.Document; break;
                            case ".jpg": case ".png": fileType = FileType.img; break;
                            default: fileType = null; break;
                        }

                        if (fileType != null)
                        {
                            string webRootPath = _env.WebRootPath;
                            var path = Path.Combine(webRootPath, _Global.PATH_TOPIC, existContribution.TopicId.ToString(), user.UserName);
                            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                            // Upload file
                            path = Path.Combine(path,/*Ép kiểu ngày*/ string.Format("{0}.{1:yyyy-MM-dd}{2}", user.UserNumber, DateTime.Now, fileExtension));//đặt tên cho file
                            var stream = new FileStream(path, FileMode.Create);
                            file.CopyTo(stream);

                            var newFile = new SubmittedFile();
                            newFile.ContributionId = existContribution.Id;
                            newFile.URL = path;
                            newFile.Type = (FileType)fileType;

                            _context.Add(newFile);
                            await _context.SaveChangesAsync();
                        }
                    }

                    //    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Details), new { id = contribution.TopicId });

            /// ViewData["ContributorId"] = userId;
            ////   ViewData["TopicId"] = new SelectList(_context.Topic, "Id", "Id", contribution.TopicId);

            ///return View(contribution);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int topicId, string commentContent)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                var existContribution = await _context.Contributions.FirstOrDefaultAsync(c => c.ContributorId == userId && c.TopicId == topicId);

                if (existContribution != null && !String.IsNullOrEmpty(commentContent))
                {
                    var comment = new Comment();
                    comment.Content = commentContent;
                    comment.Date = DateTime.Now;
                    comment.ContributionId = existContribution.Id;
                    comment.UserId = userId;
                    _context.Add(comment);
                    await _context.SaveChangesAsync();

                }

                //    return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Details), new { id = topicId });

            /// ViewData["ContributorId"] = userId;
            ////   ViewData["TopicId"] = new SelectList(_context.Topic, "Id", "Id", contribution.TopicId);

            ///return View(contribution);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUpload(Contribution contribution, int fileId)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == contribution.TopicId);

            if (topic.DeadLine_2 >= DateTime.Now)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (ModelState.IsValid)
                {

                    var fileSubmitted = await _context.SubmittedFiles.FindAsync(fileId);
                    System.IO.File.Delete(fileSubmitted.URL);


                    _context.Remove(fileSubmitted);
                    await _context.SaveChangesAsync();
                }

            }
            else
            {

            }
            return RedirectToAction(nameof(Details), new { id = contribution.TopicId });
        }
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            int topicId = 0;
            if (ModelState.IsValid)
            {

                var commented = await _context.Comments.FindAsync(commentId);
                var contribution = await _context.Contributions.FindAsync(commented.ContributionId);
                topicId = contribution.TopicId;
                _context.Remove(commented);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = topicId });
        }
    }
}
