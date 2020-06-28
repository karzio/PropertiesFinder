using DatabaseConnection;
using IntegrationApi.Controllers;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationApi.Tests
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void EntryList__EmptyList__NoContent()
        {
            // arrange
            var data = new List<Entry>().AsQueryable();

            var mockSet = new Mock<DbSet<Entry>>(); 
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DatabaseContext>();

            mockContext.Setup(m => m.Entries).Returns(mockSet.Object);

            var service = new Query(mockContext.Object);

            // act
            var entries = service.GetEntries();

            // assert
            Assert.AreEqual(0, entries.Value.Count());
        }

        [Test]
        public void EntryList__FilterCities__ShowOnlyDefaultCityGdansk()
        {
            // arrange
            var data = new List<Entry>
            {
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.KRAKOW},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 1}
                },
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 1}
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Entry>>();
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DatabaseContext>();

            mockContext.Setup(m => m.Entries).Returns(mockSet.Object);

            var service = new Query(mockContext.Object);

            // act
            var entries = service.GetEntries();

            // assert
            Assert.AreEqual(1, entries.Value.Count());
        }

        [Test]
        public void EntryList__DifferentPropertyPricesSortedDecreasingly__SortedEntryList()
        {
            // arrange
            var data = new List<Entry>
            {
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 444}
                },
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 2}
                },
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 333}
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Entry>>();
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DatabaseContext>();

            mockContext.Setup(m => m.Entries).Returns(mockSet.Object);

            var service = new Query(mockContext.Object);

            // act
            var entries = service.GetEntries();

            // assert
            Assert.AreEqual(3, entries.Value.Count());
            Assert.AreEqual(2, entries.Value.ElementAt(0).PropertyPrice.PricePerMeter);
            Assert.AreEqual(333, entries.Value.ElementAt(1).PropertyPrice.PricePerMeter);
            Assert.AreEqual(444, entries.Value.ElementAt(2).PropertyPrice.PricePerMeter);
        }

        [Test]
        public void EntryList__PlaceWithoutPropertyPrice__ShowPlacesWithPricesOnly()
        {
            // arrange
            var data = new List<Entry>
            {
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 444}
                },
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 0}
                },
                new Entry {
                    PropertyAddress = new PropertyAddress {City = PolishCity.GDANSK},
                    PropertyPrice = new PropertyPrice {PricePerMeter = 333}
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Entry>>();
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Entry>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DatabaseContext>();

            mockContext.Setup(m => m.Entries).Returns(mockSet.Object);

            var service = new Query(mockContext.Object);

            // act
            var entries = service.GetEntries();

            // assert
            Assert.AreEqual(2, entries.Value.Count());
            Assert.AreEqual(333, entries.Value.ElementAt(0).PropertyPrice.PricePerMeter);
            Assert.AreEqual(444, entries.Value.ElementAt(1).PropertyPrice.PricePerMeter);
        }

        [Test]
        public void LetterSwap__ChangePolishLettersToLatin()
        {
            // arrange
            string word = "¯Ó£Æ";

            // act
            var newWord = Query.SwapPolishLettersAndSpace(word);

            // assert
            Assert.AreEqual(newWord, "ZOLC");
        }
    }
}