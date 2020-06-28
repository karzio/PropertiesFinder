using DatabaseConnection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationApi.Controllers
{
    public class Query
    {
        private readonly int _limit;
        private readonly string _city;
        private DatabaseContext _context;

        /// <summary>
        /// Returns the cheapest entries (per meter) in the city (Gdansk by default)
        /// </summary>
        public Query(string city, DatabaseContext context)
        {
            _limit = 10;
            _city = city;
            _context = context;
        }

        public Query(DatabaseContext context)
        {
            _limit = 10;
            _city = "gdansk";
            _context = context;
        }

        public static string SwapPolishLettersAndSpace(string word)
        {
            word = word.Replace('Ą', 'A');
            word = word.Replace('Ó', 'O');
            word = word.Replace('Ń', 'N');
            word = word.Replace('Ł', 'L');
            word = word.Replace('Ę', 'E');
            word = word.Replace('Ć', 'C');
            word = word.Replace('Ż', 'Z');
            word = word.Replace('Ź', 'Z');
            word = word.Replace(' ', '_');

            return word;
        }

        public PolishCity GetPolishCity()
        {
            PolishCity polishCity;
            try
            {
                polishCity = (PolishCity)Enum.Parse(
                    typeof(PolishCity),
                    SwapPolishLettersAndSpace(_city.ToUpper()));
            }
            catch (ArgumentException)
            {
                polishCity = PolishCity.GDANSK;
            }
            return polishCity;
        }

        public ActionResult<IEnumerable<Entry>> GetEntries()
        {
            var polishCity = GetPolishCity();
            var entries = _context.Entries
                .Where(entry => entry.PropertyAddress.City == polishCity &&
                        entry.PropertyPrice.PricePerMeter > 0)
                .OrderBy(entry => entry.PropertyPrice.PricePerMeter)
                .Take(_limit)
                .Include(entry => entry.OfferDetails)
                    .ThenInclude(offerDetails => offerDetails.SellerContact)
                .Include(entry => entry.PropertyAddress)
                .Include(entry => entry.PropertyDetails)
                .Include(entry => entry.PropertyFeatures)
                .Include(entry => entry.PropertyPrice)
                .ToList();

            return entries;
        }

    }
}
