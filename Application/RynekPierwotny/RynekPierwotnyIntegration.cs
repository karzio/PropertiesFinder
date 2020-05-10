using HtmlAgilityPack;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;

namespace Application.RynekPierwotny
{
    class RynekPierwotnyIntegration : IWebSiteIntegration
    {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }
        public IEqualityComparer<Entry> EntriesComparer { get; }
        public HtmlWeb htmlWeb { get; }
        private readonly string defaultOffersQuery = "/s/nowe-mieszkania-i-domy/";
        private readonly string mainPageOfferLinkXPath = "/html/body/div[1]/div/div[2]/main/section/div[2]/div[2]/div/ul/li/div/a";
        private readonly string singleOfferLinkXPath = "/html/body/div[1]/div/div[2]/main/section/div/div[3]/div[1]/div[2]/div/div/table/tbody/tr/td[2]/a";


        public RynekPierwotnyIntegration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                // I use the mobile version of the webpage because the desktop version
                // does not hold urls to a single page offer in html (I guess it is 
                // mapped in React.js). Apart from that the page is very similar.
                Url = "http://m.rynekpierwotny.pl",
                Name = "Rynek Pierwotny Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = true,
                    HomeRental = false,
                    HouseSale = true,
                    HouseRental = false
                }
            };

            htmlWeb = new HtmlWeb();
        }

        /// <summary>
        /// Retrieve 'a' elements by an XPath.
        /// Default XPath retrieves elements that hold link to page with bundled offers.
        /// These pages have lists of offers that are connected by their
        /// address and developer (in most cases I think).
        /// </summary>
        /// <returns>HTML node collection</returns>
        private HtmlNodeCollection GetHtmlNodeCollection(string url, string XPath)
        {
            var document = htmlWeb.Load(url);
            HtmlNodeCollection htmlNodeCollection = document.DocumentNode.SelectNodes(XPath);
            return htmlNodeCollection;
        }

        private IEnumerable<HtmlNodeCollection> GetHtmlNodeCollections(HtmlNodeCollection htmlNodeCollection, string XPath)
        {
            foreach (var node in htmlNodeCollection)
            {
                var url = WebPage.Url + node.GetAttributeValue("href", "none");
                var elements = GetHtmlNodeCollection(url, XPath);
                yield return elements;
            }
        }


        private IEnumerable<HtmlDocument> GetSingleOfferDocuments(HtmlNodeCollection htmlNodes)
        {
            var singleOfferNodeCollections = GetHtmlNodeCollections(htmlNodes, singleOfferLinkXPath);
            foreach (var singleOfferNodeCollection in singleOfferNodeCollections)
            {
                foreach (var singleOfferNode in singleOfferNodeCollection)
                {
                    yield return htmlWeb.Load(WebPage.Url + singleOfferNode.GetAttributeValue("href", "none"));
                }
            }
        }

        private IEnumerable<Entry> CreateEntries()
        {
            var mainPageOfferNodeCollection = GetHtmlNodeCollection(WebPage.Url + defaultOffersQuery, mainPageOfferLinkXPath);
            var singleOfferDocuments = GetSingleOfferDocuments(mainPageOfferNodeCollection);
            foreach (var singleOfferDocument in singleOfferDocuments)
            {
                SingleOffer singleOffer = new SingleOffer(singleOfferDocument);
                yield return singleOffer.CreateEntry();
            }
        }

        public Dump GenerateDump()
        {
            return new Dump
            {
                Entries = CreateEntries(),
                DateTime = DateTime.Now,
                WebPage = WebPage,
            };
        }
    }
}
