using Application.Business.UnitOfWork;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ameen_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CommunityController(IUnitOfWork UOW)
        {
            _UnitOfWork = UOW;
        }
        [Route("GetAllCommunities")]
        [HttpGet]
        public List<Community> GetAllCommunities()
        {
            var comm = _UnitOfWork.Community.GetActiveCommunities();
            return comm;
        }
        [Route("GetCommunityById/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetCommunity(int id)
        {
            var Community = _UnitOfWork.Community.FindById(id);

            if (Community == null)
            {
                return NotFound("Community Not Found!");
            }
            var Communityinfo = await _UnitOfWork.Community.GetCommunityInfo(id);
            return Ok(Communityinfo);
        }
        [Route("AddOrEditCommunity")]
        [HttpPost]
        public async Task<IActionResult> PostCommunity(Community comm)
        {
            try
            {
                
                bool duplicate = _UnitOfWork.Community.CheckDuplicateCommunity(comm.Name, comm.Url, comm.id);
                if (duplicate)
                {
                    return BadRequest("Community name or url already exist");
                }
                else
                {
                    if (comm.id == 0)
                    {
                        comm.CreatedAt = DateTime.Now;
                        _UnitOfWork.Community.Add(comm);
                    }
                    else
                    {
                        var Community = _UnitOfWork.Community.FindById(comm.id);
                        if (Community == null)
                        {
                            return NotFound("Community Not Found!");
                        }
                        comm.UpdatedAt = DateTime.Now;
                         _UnitOfWork.Community.Update(comm);
                    }
                    await _UnitOfWork.SaveDbChanges();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [Route("DeleteCommunity")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCommunity(int? id)
        {
            if (id == null)
            {
                return NotFound("Community Not Found!");
            }
            var staff = await _UnitOfWork.Community.DeleteCommunity(id);
            if (staff == null)
            {
                return NotFound("Community Not Found!");
            }
            staff.IsActive = false;
            staff.DeletedAt = DateTime.Now;
            _UnitOfWork.Community.Update(staff);
            await _UnitOfWork.SaveDbChanges();
            return NoContent();
        }
    }
}
