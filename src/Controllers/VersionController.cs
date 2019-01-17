﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SignalRServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VersionController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet()]
        public ActionResult<Dictionary<string, string>> GetVersion()
        {
            return new Dictionary<string, string>(){
                {"machinename", Environment.MachineName },
                {"environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") },
                {"projectname", Assembly.GetEntryAssembly().GetName().Name},
                {"buildversion", Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion},
                {"buildate", (new System.IO.FileInfo(Assembly.GetExecutingAssembly().Location)).LastWriteTimeUtc.ToString()},
                {"redis", Environment.GetEnvironmentVariable("REDIS_CONFIG")}
            };
        }
    }
}