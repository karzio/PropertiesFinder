using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DatabaseConnection;
using Microsoft.AspNetCore.Mvc;
using Models;
using RynekPierwotny;
using Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IntegrationApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Page : ControllerBase
    {
        // POST api/<Page>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Entry>>> PostPage([FromBody] JsonElement body)
        {

            int pageNumber;
            try  
            {
                pageNumber = int.Parse(body.GetProperty("page").ToString());
            }
            catch (Exception)
            {
                return BadRequest("The page number provided is invalid");
            }

            RynekPierwotnyIntegration rynekPierwotnyIntegration = new RynekPierwotnyIntegration(
                new DumpFileRepository(), new RynekPierwotnyComparer());
            IEnumerable<Entry> entries = rynekPierwotnyIntegration.CreateEntries(pageNumber);

            using (DatabaseContext databaseContext = new DatabaseContext()) {
                foreach (Entry entry in entries)
                    databaseContext.Entries.Add(entry);

                await databaseContext.SaveChangesAsync();
            }

            return entries.ToList();
        }
    }
}
