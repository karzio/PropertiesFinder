using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseConnection;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IntegrationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Info : ControllerBase
    {
        // GET: api/<Info>
        [HttpGet]
        public object Get()
        {
            return new Dictionary<string, string>() {
                { "connectionString", DatabaseContext.ConnectionString},
                { "integrationName", "rynekpierwotny" },
                { "studentName", "Karol Z." },
                { "studentIndex", "149599" }
            };
        }

    }
}
