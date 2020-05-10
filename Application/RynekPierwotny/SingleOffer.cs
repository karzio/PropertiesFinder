using HtmlAgilityPack;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.RynekPierwotny
{
    class SingleOffer
    {
        public HtmlDocument HtmlDocument { get; }
        private string RawDescription { get; set; }
        private decimal Area;

        public SingleOffer(HtmlDocument htmlDocument)
        {
            HtmlDocument = htmlDocument;
        }


        private string GetElementValue(string XPath)
        {
            var node = HtmlDocument.DocumentNode.SelectSingleNode(XPath);
            return node.InnerText;
        }

        private string GetAttributeValue(string XPath, string attributeName)
        {
            var node = HtmlDocument.DocumentNode.SelectSingleNode(XPath);
            var attribute = node?.GetAttributeValue(attributeName, "none");
            return attribute;
        }

        private HtmlNode GetChildNodeByAttribute(string attributeName, HtmlNodeCollection parentNodes)
        {
            var node = parentNodes
                .Select(element => element)
                .Where(element => element.GetAttributeValue("data-header", "none") == attributeName).FirstOrDefault();
            return node;
        }

        /// <summary>
        /// Just to make sure. Most of these, if not all, ougth to be for sale.
        /// </summary>
        /// <returns></returns>
        private OfferKind GetOfferKind()
        {
            var title = GetElementValue("/html/body/div[1]/div/div[2]/main/section/div/div[1]/div[1]/header/h1");
            if (title.Contains("sprzedaż", StringComparison.OrdinalIgnoreCase) ||
                RawDescription.Contains("sprzedaż", StringComparison.OrdinalIgnoreCase))
            {
                return OfferKind.SALE;
            }
            return OfferKind.RENTAL;
        }

        /// <summary>
        /// Gets each word in a sentence and checks if it's a number.
        /// If it is then the numbers are compared and the bigger one 
        /// is returned, because sometimes there are more dates in 
        /// a text, for example a date of starting a construction.
        /// </summary>
        /// <param name="text">String with a sentence which holds dates related to construction time.</param>
        /// <returns>Year of construction</returns>
        private int GetYearOfConstruction(string text)
        {
            int year = 0;
            var words = text.Split(" ");
            foreach (string word in words)
            {
                try
                {
                    int newYear = Convert.ToInt32(word);
                    if (newYear > year)
                        year = newYear;
                }
                catch (FormatException)
                {
                    continue;
                }
            }
            return year;
        }

        private PropertyDetails CreatePropertyDetails()
        {
            var propertyDetailsNodes = HtmlDocument.DocumentNode
                .SelectNodes("/html/body/div[1]/div/div[2]/main/section/div/div[2]/div/div/table/tbody/tr/td");
            
            var areaNode = GetChildNodeByAttribute("Powierzchnia", propertyDetailsNodes);
            var area = areaNode.InnerText.Split("\u00A0")[0];
            Area = Convert.ToDecimal(area.ToString());

            var numberOfRoomsNode = GetChildNodeByAttribute("Pokoje", propertyDetailsNodes);
            var numberOfRooms = numberOfRoomsNode.InnerText.Split("\u00A0")[0];

            var floorNumberNode = GetChildNodeByAttribute("Piętro", propertyDetailsNodes);
            // Set floor number to zero if it's a house (floor number is not defined)
            var floorNumber = "0";
            if (floorNumberNode != null)
            {
                floorNumber = floorNumberNode.InnerText;
                // Every other floor, apart from the base, is a number
                if (floorNumber.ToLower() == "parter")
                    floorNumber = "0";
                else
                    floorNumber = floorNumber.Split("\u00A0")[0];
            }

            var yearOfConstructionNode = GetChildNodeByAttribute("Realizacja inwestycji", propertyDetailsNodes);
            var yearOfConstruction = GetYearOfConstruction(yearOfConstructionNode.InnerText);

            return new PropertyDetails
            {
                Area = Area,
                NumberOfRooms = Convert.ToInt32(numberOfRooms),
                FloorNumber = Convert.ToInt32(floorNumber),
                YearOfConstruction = yearOfConstruction
            };
        }

        private PropertyFeatures CreatePropertyFeatures()
        {
            string[] additionalArea;
            decimal gardenArea = 0;
            int balconies = 0;
            decimal basementArea = 0;
            int outdoorParkingPlaces = 0;
            int indoorParkingPlaces = 0;

            Regex regex = new Regex(@"(\d+)");
            var propertyDetailsNodes = HtmlDocument.DocumentNode
                .SelectNodes("/html/body/div[1]/div/div[2]/main/section/div/div[2]/div/div/table/tbody/tr/td");
            var additionalAreaNode = GetChildNodeByAttribute("Powierzchnie dodatkowe", propertyDetailsNodes);
            if (additionalAreaNode == null)
                additionalArea = RawDescription.Split('.', ',', ';');
            else 
                additionalArea = additionalAreaNode.InnerText.Split('.', ',', ';');

            foreach (var area in additionalArea)
            {
                if (area.Contains("ogródek", StringComparison.OrdinalIgnoreCase))
                {
                    Match result = regex.Match(area);
                    if (result.Success)
                        gardenArea = Convert.ToDecimal(result.Groups[1].Value);
                }
                else if (area.Contains("balkon", StringComparison.OrdinalIgnoreCase) || 
                    area.Contains("taras", StringComparison.OrdinalIgnoreCase))
                { 
                    balconies++;
                }
                else if (area.Contains("piwnicę", StringComparison.OrdinalIgnoreCase) ||
                    area.Contains("piwnica", StringComparison.OrdinalIgnoreCase))
                {
                    Match result = regex.Match(area);
                    if (result.Success)
                        basementArea = Convert.ToDecimal(result.Groups[1].Value);
                }
                else if (area.Contains("postojowe naziemne", StringComparison.OrdinalIgnoreCase))
                {
                    Match result = regex.Match(area);
                    if (result.Success)
                        indoorParkingPlaces = Convert.ToInt32(result.Groups[1].Value);
                    // If there is no specific value then increment to notify that there is a parking spot
                    if (indoorParkingPlaces == 0) indoorParkingPlaces++;
                }
                else if (area.Contains("podziemne", StringComparison.OrdinalIgnoreCase) || 
                    area.Contains("garaż", StringComparison.OrdinalIgnoreCase))
                {
                    Match result = regex.Match(area);
                    if (result.Success)
                        outdoorParkingPlaces = Convert.ToInt32(result.Groups[1].Value);
                    // If there is no specific value then increment to notify that there is a parking spot
                    if (outdoorParkingPlaces == 0) outdoorParkingPlaces++;
                }
            }

            // If data is not found in the table maybe it can be found in raw description
            if (indoorParkingPlaces == 0)
                if (RawDescription.Contains("postojowe naziemne"))
                    indoorParkingPlaces++;
            if (outdoorParkingPlaces == 0)
                if (RawDescription.Contains("podziemne") || RawDescription.Contains("garaż", StringComparison.OrdinalIgnoreCase))
                    outdoorParkingPlaces++;

            return new PropertyFeatures { 
                BasementArea = basementArea,
                Balconies = balconies,
                GardenArea = gardenArea,
                IndoorParkingPlaces = indoorParkingPlaces,
                OutdoorParkingPlaces = outdoorParkingPlaces
            };
        }

        private OfferDetails CreateOfferDetails()
        {
            var telephoneAttribute = GetAttributeValue(
                "/html/body/div[1]/div/div[2]/main/section/div/div[5]/div/div/div/div[2]/div/div/div/div/a", "href"
                );
            string telephone = "";
            if (telephoneAttribute == null)
            {
                Regex regex = new Regex(@"tel:(\d+)");
                var lookupNode = HtmlDocument.DocumentNode.SelectSingleNode(
                    "/html/body/div[1]/div/div[2]/main/section/div/div[5]/div/div/div/div[2]");
                var result = regex.Match(lookupNode.InnerHtml);
                if (result.Success)
                    telephone = result.Value.Split(':')[1];
            }
            else if (telephoneAttribute.Contains(':'))
            {
                telephone = telephoneAttribute.Split(':')[1];
            }
            // Actually, a name of a developer
            var name = GetAttributeValue(
                "/html/body/div[1]/div/div[2]/main/section/div/div[5]/div/div/div/div[1]/a/img", "alt");
            var sellerContact = new SellerContact
            {
                Telephone = telephone,
                Name = name
            };
            var document = HtmlDocument.ToString();
            // I think the only possibility that offer is still on the webpage (accessible by a user
            // who clicks his way out, not just passes an url) is when it is still valid. So I set this
            // to not available if it is reserved (but it can be still reversed if someone calls of
            // a reservation)
            var isReserved = document.Contains("Ta oferta jest obecnie zarezerwowana");
            return new OfferDetails
            {
                Url = GetAttributeValue("/html/head/link[2]", "href"),
                OfferKind = GetOfferKind(),
                IsStillValid = !isReserved,
                SellerContact = sellerContact
            };
        }

        private void GetRawDescription()
        {
            var rawDescriptionNode = HtmlDocument.DocumentNode
                .SelectSingleNode("/html/body/div[1]/div/div[2]/main/section/div/div[3]/div/div");
            RawDescription = rawDescriptionNode.InnerText;
        }

        private HtmlNode GetPriceNode()
        {
            HtmlNode priceNode;
            var propertyDetailsNodes = HtmlDocument.DocumentNode
                .SelectNodes("/html/body/div[1]/div/div[2]/main/section/div/div[2]/div/div/table/tbody/tr/td");
            priceNode = GetChildNodeByAttribute("Cena mieszkania", propertyDetailsNodes);
            if (priceNode == null)
            {
                priceNode = GetChildNodeByAttribute("Cena domu", propertyDetailsNodes);
            }
            return priceNode;
        }

        private PropertyPrice CreatePropertyPrice()
        {
            decimal totalGrossPrice;
            decimal pricePerMeter;
            var priceNode = GetPriceNode();

            // Some properties may not yet have a price
            if (priceNode == null || priceNode.InnerText == "Zapytaj o cenę")
            {
                return new PropertyPrice
                {
                    TotalGrossPrice = 0,
                    PricePerMeter = 0,
                    ResidentalRent = 0
                };
            }

            Regex regex = new Regex(@"(\d+\s)+");
            var priceText = priceNode.InnerText.Split(',', ' ');
            List<int> prices = new List<int>();
            foreach (var price in priceText)
            {
                Match result = regex.Match(price);
                if (result.Success) 
                {
                    var newPrice = result.ToString().Replace("\u00A0", String.Empty);
                    prices.Add(Convert.ToInt32(newPrice)); 
                }
            }
            totalGrossPrice = Convert.ToInt32(prices[0]);
            if (Area > 0)
                pricePerMeter = totalGrossPrice / Area;
            else
                pricePerMeter = 0;

            return new PropertyPrice
            {
                TotalGrossPrice = totalGrossPrice,
                PricePerMeter = pricePerMeter,
                ResidentalRent = 0
            };
        }

        /// <summary>
        /// Helper method to swap the letters from polish to latin to make possible
        /// parsing the enum values.
        /// </summary>
        /// <param name="word">String in which letters should be swapped</param>
        /// <returns>String with swapped letters</returns>
        private string SwapPolishLettersAndSpace(string word)
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

        private PropertyAddress CreatePropertyAddress()
        {
            // I do realize that this piece of code is not pretty
            // but I have no idea how to make it cleaner
            var propertyDetailsNodes = HtmlDocument.DocumentNode
                .SelectNodes("/html/body/div[1]/div/div[2]/main/section/div/div[2]/div/div/table/tbody/tr/td");
            var detailedAddressNode = GetChildNodeByAttribute("Oznaczenie", propertyDetailsNodes);
            var detailedAddress = detailedAddressNode.InnerText;

            var propertyAddressXPath = "/html/body/div[1]/div/div[2]/main/section/div/div[1]/div[1]/div/div[1]/h2";
            var propertyAddressText = GetElementValue(propertyAddressXPath);
            var addressParts = propertyAddressText.Split(',', '.', ' ');
            var street = addressParts.LastOrDefault();
            var city = addressParts.FirstOrDefault().ToUpper();
            var cityParsed = SwapPolishLettersAndSpace(city);
            var district = addressParts.ElementAtOrDefault(2);

            PolishCity cityEnum;
            try
            {
                cityEnum = (PolishCity)Enum.Parse(typeof(PolishCity), cityParsed);
            }
            catch (ArgumentException)
            {
                try
                {
                    var districtParsed = SwapPolishLettersAndSpace(district.ToUpper());
                    cityEnum = (PolishCity)Enum.Parse(typeof(PolishCity), districtParsed);
                }
                catch (ArgumentException)
                { 
                    return new PropertyAddress
                    {
                        District = district,
                        StreetName = street,
                        DetailedAddress = detailedAddress
                    };
                }
            }
            return new PropertyAddress
            {
                City = cityEnum,
                District = district,
                StreetName = street,
                DetailedAddress = detailedAddress
            };
        }

        public Entry CreateEntry()
        {
            GetRawDescription();
            var entry = new Entry
            {
                OfferDetails = CreateOfferDetails(),
                PropertyDetails = CreatePropertyDetails(),
                PropertyFeatures = CreatePropertyFeatures(),
                RawDescription = RawDescription,
                PropertyAddress = CreatePropertyAddress(),
                PropertyPrice = CreatePropertyPrice()
            };
            return entry;
        }

    }
}
