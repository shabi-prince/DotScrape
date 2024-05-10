using Application.Business.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ameen_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _UnitOfWork;

        public DashboardController(IUnitOfWork UOW)
        {
            _UnitOfWork = UOW;
        }
        [HttpGet]
        [Route("GetEventCommunities")]
        public IActionResult GetTotalEventCommunities()
        {
           var res=_UnitOfWork.dashboard.GetCountOfEventCommunities();
            return Ok(res);
        }
    }
}
