using Domain.Entities;
using HorseRider_ERP.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ameen_UI.Controllers
{
    public class EventController : Controller
    {
        private readonly IConfiguration _Configuration;

        public EventController(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            ViewBag.token = Token;


            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            List<Event> Events = new();
            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Accept.Clear();
                evnt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Event/GetAllEvents");
                if (response.IsSuccessStatusCode)
                {
                    Events = await response.Content.ReadAsAsync<List<Event>>();
                }
            }
            return View(Events);
        }

        public async Task<IActionResult> ScrapEvents(string url,int id)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }

            using (var evnt = new HttpClient())
            {
                evnt.Timeout = TimeSpan.FromMinutes(250);
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Event/ScrapeEvents?url=" + url + "&id=" + id);
                if (!response.IsSuccessStatusCode)
                {
                    //TempData["Error"] = "Something went wrong!";
                    return StatusCode(500);
                }

            }
            return Ok();
        }

        public async Task<IActionResult> ApproveAllPendingEvents()
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }

            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Event/ApprovePendingEvents");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500);
                }

            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DenyyStatus(int id)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }

            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Event/DenyStatus/?id=" + id);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500);
                }

            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            Event Evnt = new();
            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Accept.Clear();
                evnt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Event/EventDetail/" + id);
                if (response.IsSuccessStatusCode)
                {
                    Evnt = await response.Content.ReadAsAsync<Event>();
                }
            }
            return View(Evnt);
        }
    }
}
