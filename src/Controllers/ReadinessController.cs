using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatTest.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReadinessController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet()]
        public ActionResult<Dictionary<string, string>> GetVersion()
        {
            if (Startup.Readiness >= 2)
            {
                return Ok(Startup.Readiness);
            }
            return BadRequest(Startup.Readiness);
        }
    }
}
