using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatTest.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadinessController : Controller
    {
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
