using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RuanMou.MicroService.TeamService.Controllers
{
    /// <summary>
    /// consul心跳检测地址
    /// </summary>
    [Route("HealthCheck")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        // GET: api/Teams
        [HttpGet]
        public ActionResult GetHealthCheck()
        {
            Console.WriteLine("进行心跳检测");
            return Ok("连接正常");
        }
    }
}