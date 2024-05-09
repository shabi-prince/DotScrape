using Domain.Entities;
using HorseRider_ERP.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ameen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly SignInManager<ApplicationUsers> _signInManager;
        private readonly IConfiguration _Configuration;

        public AccountController(SignInManager<ApplicationUsers> signInManager,
            UserManager<ApplicationUsers> userManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _Configuration = config;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = _userManager.FindByEmailAsync(Email);
                string secret = _Configuration["JWT:Key"];
                string issuer = _Configuration["JWT:Issuer"];
                var Token = GenerateToken.Authenticate(user.Result, secret, issuer);
                return Ok(Token);
            }
            else
            {
                return Unauthorized("Invalid Login Attempt");
            }
        }
    }
}
