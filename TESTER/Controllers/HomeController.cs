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
        //All events in webapp
        private static List<Event> allEvents = new List<Event>
        {
            new Event
            {
                Id = 1,
                Title = "Sanlam Cape Town Marathon",
                Date = new DateTime(2025, 10, 18),
                Category = "Sports",
                Description = "The annual marathon starting at Green Point Stadium and running through Sea Point, the City Bowl, and past iconic Table Mountain views. Suitable for both professional and amateur runners."
            },
            new Event
            {
                Id = 2,
                Title = "Cape Town Carnival",
                Date = new DateTime(2025, 3, 15),
                Category = "Culture",
                Description = "A vibrant parade along Green Point Fan Walk, featuring dancers, musicians, and elaborate floats that celebrate Cape Town’s diverse communities and heritage."
            },
            new Event
            {
                Id = 3,
                Title = "Rocking the Daisies Festival",
                Date = new DateTime(2025, 10, 2),
                Category = "Music",
                Description = "Held at Cloof Wine Estate in Darling, just outside Cape Town, this multi-day music and lifestyle festival includes local and international performers, camping, and sustainability workshops."
            },
            new Event
            {
                Id = 4,
                Title = "Arts & Crafts Market at Kirstenbosch Gardens",
                Date = new DateTime(2025, 11, 12),
                Category = "Arts",
                Description = "A relaxed outdoor market inside Kirstenbosch National Botanical Garden showcasing handmade crafts, local artwork, and organic food stalls."
            },
            new Event
            {
                Id = 5,
                Title = "Open Streets Langa",
                Date = new DateTime(2025, 10, 26),
                Category = "Community",
                Description = "Langa Main Road transforms into a car-free zone filled with street performers, food stalls, art installations, and community discussions promoting active citizenship."
            },
            new Event
            {
                Id = 6,
                Title = "Cape Town Food & Farmers Market",
                Date = new DateTime(2025, 11, 1),
                Category = "Food",
                Description = "Hosted at the Oranjezicht City Farm Market at Granger Bay, near the V&A Waterfront, this market offers fresh produce, gourmet dishes, and locally sourced products."
            },
            new Event
            {
                Id = 7,
                Title = "Cape Town International Jazz Festival",
                Date = new DateTime(2025, 4, 25),
                Category = "Music",
                Description = "Africa’s grandest jazz gathering held at the Cape Town International Convention Centre (CTICC), featuring world-renowned and local jazz musicians."
            },
            new Event
            {
                Id = 8,
                Title = "Two Oceans Swim Challenge",
                Date = new DateTime(2025, 12, 5),
                Category = "Sports",
                Description = "A thrilling open-water swimming event starting at Clifton 4th Beach and finishing at Sea Point Promenade, offering scenic ocean views and community fun."
            },
            new Event
            {
                Id = 9,
                Title = "Khayelitsha Book Festival",
                Date = new DateTime(2025, 9, 26),
                Category = "Culture",
                Description = "A community-driven event at Lookout Hill, Khayelitsha, promoting reading and storytelling through author talks, local publishers, and interactive workshops."
            },
            new Event
            {
                Id = 10,
                Title = "First Thursdays Cape Town",
                Date = new DateTime(2025, 10, 2),
                Category = "Arts",
                Description = "Held in the Cape Town CBD and Bree Street area, this monthly event sees art galleries, bars, and cultural spaces open late for the public to explore city nightlife creatively."
            },
            new Event
            {
                Id = 11,
                Title = "Community Beach Clean-Up",
                Date = new DateTime(2025, 10, 20),
                Category = "Environment",
                Description = "Organised by local volunteers at Muizenberg and Bloubergstrand beaches, this initiative focuses on keeping Cape Town’s coastlines clean and plastic-free."
            },
            new Event
            {
                Id = 12,
                Title = "Heritage Day Celebration at Company’s Garden",
                Date = new DateTime(2025, 9, 24),
                Category = "Culture",
                Description = "An outdoor celebration in Company’s Garden, Cape Town, featuring local cuisine, dance performances, and exhibitions highlighting South Africa’s heritage and unity."
            },
            new Event
            {
                Id = 13,
                Title = "Cape Malay Cooking Workshop",
                Date = new DateTime(2025, 11, 10),
                Category = "Food",
                Description = "Held in Bo-Kaap, this interactive workshop offers hands-on lessons in traditional Cape Malay cooking, including samoosas, bredie, and koeksisters."
            },
            new Event
            {
                Id = 14,
                Title = "Sunset Concert at V&A Waterfront Amphitheatre",
                Date = new DateTime(2025, 10, 5),
                Category = "Music",
                Description = "A free outdoor concert at the V&A Waterfront featuring local artists performing against the backdrop of Table Mountain during sunset."
            },
            new Event
            {
                Id = 15,
                Title = "City of Cape Town Public Meeting: Urban Renewal",
                Date = new DateTime(2025, 10, 15),
                Category = "Government",
                Description = "Hosted at the Cape Town Civic Centre, this public meeting invites residents to discuss upcoming infrastructure, housing, and urban development plans."
            }
        };

        //Data Structures used in local events
        private static Stack<Event> recentEvents = new Stack<Event>();
        private static Dictionary<string, List<Event>> eventsByCategory = allEvents.GroupBy(e => e.Category).ToDictionary(g => g.Key, g => g.ToList());
        private static HashSet<string> categories = new HashSet<string>(allEvents.Select(e => e.Category));
        private static Dictionary<string, int> userSearchPatterns = new Dictionary<string, int>();

        //Data Structures used in Add Report
        private static Stack<ReportModel> recentReports = new Stack<ReportModel>();
        private static Queue<ReportModel> reportQueue = new Queue<ReportModel>();
        private static LinkedList<ReportModel> linkedReports = new LinkedList<ReportModel>();

        //homepage
        public IActionResult Index() => View();

        //Local Events section
        public IActionResult LocalEvents(string search, string startDate, string endDate)
        {
            DateTime? start = null;
            DateTime? end = null;

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var s))
                start = s.Date;
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var e))
                end = e.Date;

            List<Event> filteredEvents = allEvents;

            if (!string.IsNullOrWhiteSpace(search) || start.HasValue || end.HasValue)
            {
                filteredEvents = allEvents
                    .Where(ev =>
                        (string.IsNullOrEmpty(search) || ev.Category.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                        (!start.HasValue || ev.Date >= start.Value) &&
                        (!end.HasValue || ev.Date <= end.Value))
                    .ToList();
            }

            // Track searches for recommendations
            if (!string.IsNullOrEmpty(search))
            {
                if (userSearchPatterns.ContainsKey(search))
                    userSearchPatterns[search]++;
                else
                    userSearchPatterns[search] = 1;
            }

          
            List<Event> recommended = new List<Event>();

           
            if (userSearchPatterns.Count > 0)
            {
                string topCategory = userSearchPatterns
                    .OrderByDescending(x => x.Value)
                    .First().Key;

                if (eventsByCategory.ContainsKey(topCategory))
                    recommended = eventsByCategory[topCategory]
                        .Where(e => !recentEvents.Contains(e))
                        .Take(3)
                        .ToList();
            }
           
            else if (recentEvents.Count > 0)
            {
                var lastViewed = recentEvents.Peek();
                if (eventsByCategory.ContainsKey(lastViewed.Category))
                    recommended = eventsByCategory[lastViewed.Category]
                        .Where(e => e.Id != lastViewed.Id)
                        .Take(3)
                        .ToList();
            }
        
            else
            {
                recommended = allEvents
                    .OrderBy(e => e.Date)
                    .Take(3)
                    .ToList();
            }

            ViewBag.Recommended = recommended;
            ViewBag.Recent = recentEvents.ToList();
            ViewBag.Categories = categories;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;

            return View(filteredEvents);
        }

        public IActionResult ViewEvent(int id)
        {
            var ev = allEvents.FirstOrDefault(e => e.Id == id);
            if (ev != null)
                recentEvents.Push(ev);

            return View(ev);
        }

        //Add Report Section
        public IActionResult AddReports()
        {
            return View();
        }

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

            // Save report
            recentReports.Push(report);
            reportQueue.Enqueue(report);
            linkedReports.AddLast(report);

            return RedirectToAction("ViewReports");
        }

        //View Report
        public IActionResult ViewReports()
        {
            ViewBag.Reports = linkedReports;
            return View();
        }
    }
}
