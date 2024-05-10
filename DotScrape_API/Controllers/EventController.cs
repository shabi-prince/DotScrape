using Application.Business.UnitOfWork;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Infrastructure;
using MoreLinq;
using System.Text.RegularExpressions;

namespace Ameen_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private IConfiguration config;
        private readonly RiderDbContext _context;
        public EventController(IUnitOfWork UOW, IConfiguration _config, RiderDbContext context)
        {
            _UnitOfWork = UOW;
            this.config = _config;
            _context = context;
        }
        [Route("GetAllEvents")]
        [HttpGet]
        public List<Event> GetAllEvents()
        {
            var events = _UnitOfWork.Event.GetActiveEvents();
            return events;
        }
        //[Route("GetCommunities")]
        //[HttpGet]
        //public IActionResult GetCommunities()
        //{
        //    var communities = _context.Communities.Where(x => x.IsActive == true).ToList();
        //    return Ok(communities);

        //}
        [Route("ScrapeEvents")]
        [HttpGet]
        public IActionResult ScrapeEvents(string url, int id)
        {
            string regular = @"^(ht|f|sf)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$";

            try
            {
                List<Event> EventsfromDB = null;
                //List<Event> DeleteEvents = new List<Event>();
                List<Event> ScrappedEvents = new List<Event>();
                //var communities = _UnitOfWork.Community.GetActiveCommunities();
                if (id > 0)
                {
                     EventsfromDB = _UnitOfWork.Event.GetActiveEventsForScrapping(id);
                }
                else
                {
                    return BadRequest("Id Not Found!");
                }             
                //var resss = Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
                //if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                //{
                    
                //}
                //_UnitOfWork.Event.ScrapeEvents(Communities);           
                //foreach (var comunty in communities)
                //{
                //WebDriver driver = new ChromeDriver();
                ChromeOptions options = new ChromeOptions();
                //set your chromeoptions here                    
                Uri uri = new Uri(config["uriLoc:uriAddress"]);
                var facebookUrl = new Uri(config["facebookCredentials:uri"]);
                string userNameOrEmail = config.GetSection("facebookCredentials").GetSection("email").Value;
                string password = config.GetSection("facebookCredentials").GetSection("password").Value;
                WebDriver driver = new RemoteWebDriver(uri, options.ToCapabilities(), TimeSpan.FromMinutes(250));
                driver.Navigate().GoToUrl(facebookUrl);
                var email = driver.FindElement(By.Name("email"));
                email.Clear();
                email.SendKeys(userNameOrEmail);
               
                var loginbutton = driver.FindElement(By.Name("login"));
                loginbutton.Click();
                Thread.Sleep(1000);
               
              
                //Thread.Sleep(Convert.ToInt32(config["Time:sleepTime"]));
                Thread.Sleep(1000);
                var items = driver.FindElements(By.CssSelector("div._5zma > div > div > div > div > div > div > a"));
               
                foreach (var item in items)
                {
                    Event evnt = new Event();
                    //WebDriver drivers = new ChromeDriver();
                    ChromeOptions optionsInner = new ChromeOptions();
                    Uri uriInner = new Uri(config["uriLoc:uriAddress"]);
                    WebDriver drivers = new RemoteWebDriver(uriInner, optionsInner.ToCapabilities(), TimeSpan.FromMinutes(250));

                    drivers.Navigate().GoToUrl(facebookUrl);
                    var email2 = drivers.FindElement(By.Name("email"));
                    email2.Clear();
                    email2.SendKeys(userNameOrEmail);
                    var passwordKey2 = drivers.FindElement(By.Name("pass"));
                    passwordKey2.Clear();
                    passwordKey2.SendKeys(password);
                    var loginbutton2 = drivers.FindElement(By.Name("login"));
                    loginbutton2.Click();
                    Thread.Sleep(1000);
                    drivers.Navigate().GoToUrl(item.GetAttribute("href"));
                    Thread.Sleep(1000);

                   
                    
                   try
                    {
                        //evnt.CoverPicture = drivers.FindElement(By.CssSelector("a._39pi._1mh-._2j7w > img")).GetAttribute("src");
                        evnt.CoverPicture = drivers.FindElement(By.CssSelector("a._39pi._1mh-._2j7w > i")).GetAttribute("style").Split(')')[0].Split('(')[1].Replace('"', ' ');
                    }
                    catch
                    {
                        evnt.CoverPicture = null;
                    }
                    //evnt.Status = "Pending";
                    ScrappedEvents.Add(evnt);
                  
                    drivers.Close();
                }
               
                if (ScrappedEvents.Count > 0)
                {
                    _UnitOfWork.Event.SaveEvents(ScrappedEvents, EventsfromDB);
                    driver.Close();
                }
                else
                {
                    driver.Close();
                }
               
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [Route("ApprovePendingEvents")]
        [HttpGet]
        public IActionResult ApprovePendingEvents()
        {
            _UnitOfWork.Event.ApproveAllPendingEvents();
            return Ok();
        }

        [Route("ApproveStatus")]
        [HttpGet]
        public async Task<IActionResult> ApproveStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var staff = await _UnitOfWork.Event.GetEventById(id);
            if (staff == null)
            {
                return NotFound();
            }
            staff.Status = "Approved";
            staff.UpdatedAt = DateTime.Now;
            _UnitOfWork.Event.Update(staff);
            await _UnitOfWork.SaveDbChanges();
            return NoContent();
        }

        

        [Route("GetEventsByStatus")]
        [HttpGet]
        public IActionResult GetEventsByStatus(int? filter)
        {
            try
            {
                if (filter <= 3)
                {
                    var events = _UnitOfWork.Event.GetEventsByStatus(filter);
                    return Ok(events);
                }
                else
                {
                    return BadRequest("Invalid Filter");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        
    }
}
