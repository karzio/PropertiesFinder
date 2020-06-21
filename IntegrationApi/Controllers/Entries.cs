using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseConnection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IntegrationApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class Entries: ControllerBase
    {
        [Route("entries")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entry>>> Get(int pageId = 1, int pageLimit = 20)
        {
            using (DatabaseContext databaseContext = new DatabaseContext())
            {
                int skip = (pageId - 1) * pageLimit;

                var entries = await databaseContext.Entries
                .Include(entry => entry.OfferDetails)
                    .ThenInclude(offerDetails => offerDetails.SellerContact)
                .Include(entry => entry.PropertyAddress)
                .Include(entry => entry.PropertyDetails)
                .Include(entry => entry.PropertyFeatures)
                .Include(entry => entry.PropertyPrice)
                .Skip(skip)
                .Take(pageLimit)
                .ToListAsync();

                return entries;
            }
        }

        // GET api/<ValuesController>/5
        [HttpGet("entry/{id}")]
        public async Task<ActionResult<Entry>> Get(int id)
        {
            using (DatabaseContext databaseContext = new DatabaseContext())
            {
                var entry = await databaseContext.Entries.FindAsync(id);

                if (entry == null)
                    return NotFound();

                await databaseContext.Entry(entry).Reference(entry => entry.PropertyAddress).LoadAsync();
                await databaseContext.Entry(entry).Reference(entry => entry.PropertyDetails).LoadAsync();
                await databaseContext.Entry(entry).Reference(entry => entry.PropertyFeatures).LoadAsync();
                await databaseContext.Entry(entry).Reference(entry => entry.PropertyPrice).LoadAsync();

                return entry;
            }
        }

        [HttpPut("entry/{id}")]
        public async Task<ActionResult<Entry>> UpdateEntry(int id, Entry entry)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            if (id != entry.Id)
            {
                return BadRequest();
            }

            using (DatabaseContext databaseContext = new DatabaseContext())
            {

                databaseContext.Entry(entry).State = EntityState.Modified;
                var existingEntry = databaseContext.Entries
                    .Where(e => e.Id == entry.Id)
                    .FirstOrDefault<Entry>();

                try
                {
                    await databaseContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (existingEntry == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
        }

    }
}
