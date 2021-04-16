using CMSCore.Data;
using CMSCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CMSCore.Controllers
{
    [Area("Guest")]
    [Authorize(Roles = "Guest")]
    [Route("api/report")]
    [ApiController]
    public class ReportAPI : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportAPI(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("department_contribution")]
        [Produces("application/json")]
        public async Task<IActionResult> Department_Contribution()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var topicIds = await _context.Topics.Where(s => s.CreationDate.Year == currentYear).Select(s => s.Id).ToListAsync();
                var contributions = await _context.Contributions.Where(c => topicIds.Contains(c.TopicId)).ToListAsync();

                List<API_Department_Contribution> statistics = new List<API_Department_Contribution>();
                foreach(var department in await _context.Departments.ToListAsync())
                {
                    var contributorIds = await _context.Users.Where(u => u.DepartmentId == department.Id)
                                                             .Select(u => u.Id)
                                                             .ToListAsync();
                    var totalContribution = contributions.Where(c => contributorIds.Contains(c.ContributorId))
                                                         .Count();

                    var temp = new API_Department_Contribution()
                    {
                        DepartmentName = department.Name,
                        TotalContribution = totalContribution
                    };
                    statistics.Add(temp);
                }
                return Ok(statistics);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet("status_contribution")]
        [Produces("application/json")]
        public async Task<IActionResult> Status_Contribution()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var topicIds = await _context.Topics.Where(s => s.CreationDate.Year == currentYear).Select(s => s.Id).ToListAsync();
                var contributions = await _context.Contributions.Where(c => topicIds.Contains(c.TopicId)).ToListAsync();

                var statusapprovedIds = await _context.Contributions.Where(a => a.Status == ContributionStatus.Approved).Select(a => a.Status).ToListAsync();
                var approveds = await _context.Contributions.Where(u => statusapprovedIds.Contains(u.Status)).ToListAsync();

                var statuspendingIds = await _context.Contributions.Where(t => t.Status == ContributionStatus.Pending).Select(t => t.Status).ToListAsync();
                var pendings = await _context.Contributions.Where(i => statuspendingIds.Contains(i.Status)).ToListAsync();

                var statusrejectedIds = await _context.Contributions.Where(x => x.Status == ContributionStatus.Rejected).Select(x => x.Status).ToListAsync();
                var rejecteds = await _context.Contributions.Where(c => statusrejectedIds.Contains(c.Status)).ToListAsync();

                List<API_Status_Contribution> statistics = new List<API_Status_Contribution>();
                foreach (var topic in await _context.Topics.ToListAsync())
                {
                    var contributorStatus = await _context.Contributions.Where(j => j.TopicId == topic.Id)
                                                             .Select(j => j.Id)
                                                             .ToListAsync();
        
                    var totalapproved = approveds.Where(l => contributorStatus.Contains(l.Id))
                                                         .Count();
                    var totalpending = pendings.Where(k => contributorStatus.Contains(k.Id))
                                                         .Count();
                    var totalrejected = rejecteds.Where(n => contributorStatus.Contains(n.Id))
                                                         .Count();

                    var temp = new API_Status_Contribution
                    {
                        TopicName = topic.Title,
                        TotalApproved = totalapproved,
                        TotalPending = totalpending,
                        TotalRejected = totalrejected
                    };
                    statistics.Add(temp);
                }
                return Ok(statistics);
            }
            catch
            {
                return BadRequest();
            }

        }


    }
}
