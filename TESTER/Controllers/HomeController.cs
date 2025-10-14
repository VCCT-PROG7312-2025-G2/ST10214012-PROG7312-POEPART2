using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TESTER.Models;

namespace TESTER.Controllers
{
    public class HomeController : Controller
    {
        // ------------------------------
        // REPORTS DATA STRUCTURES
        // ------------------------------
        private static Stack<ReportModel> recentReports = new Stack<ReportModel>();
        private static Queue<ReportModel> reportQueue = new Queue<ReportModel>();
        private static LinkedList<ReportModel> linkedReports = new LinkedList<ReportModel>();

        // ------------------------------
        // MUNICIPAL EVENTS DATA STRUCTURES
        // ------------------------------
        private static List<Event> allEvents = new List<Event>
{
    new Event
    {
        Id = 1,
        Title = "Municipal Town Hall Meeting",
        Date = new DateTime(2025, 10, 10),
        Category = "Government",
        Description = "Residents are invited to discuss local development projects and community concerns."
    },
    new Event
    {
        Id = 2,
        Title = "Local Heritage Festival",
        Date = new DateTime(2025, 11, 15),
        Category = "Culture",
        Description = "Celebrating the municipality's history, culture, and local artisans."
    },
    new Event
    {
        Id = 3,
        Title = "Community Health Drive",
        Date = new DateTime(2025, 12, 5),
        Category = "Health",
        Description = "Free health check-ups and wellness programs for all residents."
    },
    new Event
    {
        Id = 4,
        Title = "Municipal Christmas Celebration",
        Date = new DateTime(2025, 12, 20),
        Category = "Community",
        Description = "Family-friendly holiday activities, performances, and festive markets."
    },
    new Event
    {
        Id = 5,
        Title = "Environmental Awareness Campaign",
        Date = new DateTime(2025, 10, 25),
        Category = "Environment",
        Description = "A day dedicated to promoting recycling, clean energy, and green community initiatives."
    },
    new Event
    {
        Id = 6,
        Title = "Youth Sports Tournament",
        Date = new DateTime(2025, 11, 5),
        Category = "Sports",
        Description = "Local schools and youth teams compete in various sports to encourage fitness and teamwork."
    },
    new Event
    {
        Id = 7,
        Title = "Public Safety Awareness Day",
        Date = new DateTime(2025, 10, 18),
        Category = "Safety",
        Description = "Emergency services showcase safety procedures, fire prevention, and crime awareness tips."
    },
    new Event
    {
        Id = 8,
        Title = "Municipal Job Fair",
        Date = new DateTime(2025, 11, 10),
        Category = "Employment",
        Description = "Connecting job seekers with local businesses, government departments, and training programs."
    },
    new Event
    {
        Id = 9,
        Title = "Community Clean-Up Drive",
        Date = new DateTime(2025, 10, 20),
        Category = "Environment",
        Description = "Residents join hands to clean parks, beaches, and public spaces to beautify the town."
    },
    new Event
    {
        Id = 10,
        Title = "Arts and Crafts Exhibition",
        Date = new DateTime(2025, 11, 25),
        Category = "Culture",
        Description = "Local artists and crafters display their creative works and conduct workshops."
    },
    new Event
    {
        Id = 11,
        Title = "Senior Citizens Appreciation Day",
        Date = new DateTime(2025, 12, 1),
        Category = "Community",
        Description = "Honoring elderly residents with activities, performances, and special recognitions."
    },
    new Event
    {
        Id = 12,
        Title = "Municipal Budget Presentation",
        Date = new DateTime(2025, 10, 30),
        Category = "Government",
        Description = "Public presentation and discussion of the upcoming year’s municipal budget."
    },
    new Event
    {
        Id = 13,
        Title = "Women in Leadership Forum",
        Date = new DateTime(2025, 11, 8),
        Category = "Community",
        Description = "Empowering women through talks, mentorship, and leadership networking opportunities."
    },
    new Event
    {
        Id = 14,
        Title = "Local Food and Farmers Market",
        Date = new DateTime(2025, 10, 19),
        Category = "Agriculture",
        Description = "Promoting local farmers and food producers with fresh produce and artisanal goods."
    },
    new Event
    {
        Id = 15,
        Title = "Disaster Preparedness Workshop",
        Date = new DateTime(2025, 11, 20),
        Category = "Safety",
        Description = "Training residents on emergency response, first aid, and disaster management."
    }
};


        // STACK 
        private static Stack<Event> recentEvents = new Stack<Event>();

        // QUEUE
        private static Queue<Event> upcomingEvents = new Queue<Event>(allEvents.OrderBy(e => e.Date));

        // DICTIONARY
        private static Dictionary<string, List<Event>> eventsByCategory =
            allEvents.GroupBy(e => e.Category).ToDictionary(g => g.Key, g => g.ToList());

        // HASH-SET
        private static HashSet<string> categories = new HashSet<string>(allEvents.Select(e => e.Category));

        // Track user search patterns
        private static Dictionary<string, int> userSearchPatterns = new Dictionary<string, int>();

        // ------------------------------
        // DEFAULT ROUTES
        // ------------------------------
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddReports()
        {
            return View();
        }

        // ------------------------------
        // REPORTS FUNCTIONALITY
        // ------------------------------
        [HttpPost]
        public IActionResult AddReports(string location, string category, string description, IFormFile media)
        {
            string mediaPath = null;

            if (media != null && media.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{DateTime.Now.Ticks}_{Path.GetFileName(media.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    media.CopyTo(stream);
                }

                mediaPath = "/uploads/" + fileName;
            }

            var report = new ReportModel
            {
                Location = location,
                Category = category,
                Description = description,
                MediaPath = mediaPath,
                SubmittedAt = DateTime.Now
            };

            // Add to all data structures
            recentReports.Push(report);
            reportQueue.Enqueue(report);
            linkedReports.AddLast(report);

            return RedirectToAction("ViewReports");
        }

        public IActionResult ViewReports()
        {
            ViewBag.Reports = linkedReports;
            return View();
        }

        // ------------------------------
        // MUNICIPAL EVENTS FUNCTIONALITY
        // ------------------------------
        public IActionResult LocalEvents(string search)
        {
            List<Event> filteredEvents = allEvents;

            // ? Search filter
            if (!string.IsNullOrEmpty(search))
            {
                filteredEvents = allEvents
                    .Where(e => e.Category.ToLower().Contains(search.ToLower()) ||
                                e.Date.ToShortDateString().Contains(search))
                    .ToList();

                // Track search pattern
                if (userSearchPatterns.ContainsKey(search))
                    userSearchPatterns[search]++;
                else
                    userSearchPatterns[search] = 1;
            }

            // ? Recommendation system
            List<Event> recommended = new List<Event>();
            if (userSearchPatterns.Count > 0)
            {
                string topCategory = userSearchPatterns.OrderByDescending(x => x.Value).First().Key;
                if (eventsByCategory.ContainsKey(topCategory))
                {
                    recommended = eventsByCategory[topCategory];
                }
            }
            ViewBag.Recommended = recommended;
            ViewBag.Recent = recentEvents.Take(3).ToList();
            ViewBag.Categories = categories;

            return View(filteredEvents);
        }

        // View details of a single event
        public IActionResult ViewEvent(int id)
        {
            var ev = allEvents.FirstOrDefault(e => e.Id == id);
            if (ev != null)
            {
                // Add to recently viewed (Stack)
                recentEvents.Push(ev);
            }
            return View(ev);
        }
    }
}
