using Domain.Entities;
using HorseRider_ERP.Common.Utilities;
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
    public class CommunityController : Controller
    {
        private readonly IConfiguration _Configuration;

        public CommunityController(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            List<Community> Communities = new();
            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Accept.Clear();
                evnt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Community/GetAllCommunities");
                if (response.IsSuccessStatusCode)
                {
                    Communities = await response.Content.ReadAsAsync<List<Community>>();
                }
            }
            return View(Communities);
        }

        public async Task<IActionResult> _Create(int id)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            Community comm = new();
            if (id == 0)
            {
                return PartialView(comm);
            }
            else
            {
                using (var staffs = new HttpClient())
                {
                    staffs.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                    staffs.DefaultRequestHeaders.Clear();
                    staffs.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    staffs.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    HttpResponseMessage response = await staffs.GetAsync("/api/Community/GetCommunityById/" + id);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode(500);
                    }
                    comm = await response.Content.ReadAsAsync<Community>();
                }
                return PartialView(comm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Community community)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            var UserId = HttpContext.Session.GetObject<string>("UserId");
            if (community.id == 0)
            {
                community.CreatedBy = UserId;
            }
            else
            {
                community.UpdatedBy = UserId;
            }

            using (var staffs = new HttpClient())
            {
                staffs.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                staffs.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await staffs.PostAsJsonAsync<Community>("/api/Community/AddOrEditCommunity", community);
                if ((int)response.StatusCode == 200)
                {
                    return Ok();
                }
                else
                {
                    string Message = await response.Content.ReadAsStringAsync();
                    return BadRequest(Message);
                }
            }
            //return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            using (var staff = new HttpClient())
            {
                staff.BaseAddress = new Uri(_Configuration.GetSection("Api_Link").Value);
                staff.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage DeleteStaff = await staff.DeleteAsync("/api/Community/DeleteCommunity/?id=" + id);

                if (!DeleteStaff.IsSuccessStatusCode)
                {
                    return BadRequest();
                }
            }
            return Json("OK");
        }
        public async Task<IActionResult> GetAllCommunities()
        {
            string Token = HttpContext.Session.GetObject<string>("Token");
            if (string.IsNullOrWhiteSpace(Token))
            {
                TempData["TokenTimeout"] = "Your login session has ended. Please log in again.";
                return LocalRedirect("/Identity/Account/Login");
            }
            List<Community> Communities = new();
            using (var evnt = new HttpClient())
            {
                evnt.BaseAddress = new Uri(_Configuration.GetSection("API_Link").Value);
                evnt.DefaultRequestHeaders.Accept.Clear();
                evnt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                evnt.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await evnt.GetAsync("/api/Community/GetAllCommunities");
                if (response.IsSuccessStatusCode)
                {
                    Communities = await response.Content.ReadAsAsync<List<Community>>();
                }
                else
                {
                    return StatusCode(500);
                }
            }
            return Ok(Communities);
        }
    }
}
