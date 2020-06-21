using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RynekPierwotny
{
    public class RynekPierwotnyComparer : IEqualityComparer<Entry>
    {
        private bool HasTheSameAddress(Entry x, Entry y)
        {
            if (x.PropertyAddress.City.Equals(y.PropertyAddress.City) &&
                x.PropertyAddress.DetailedAddress.Equals(y.PropertyAddress.DetailedAddress) &&
                x.PropertyAddress.District.Equals(y.PropertyAddress.District) &&
                x.PropertyAddress.StreetName.Equals(y.PropertyAddress.StreetName))
                return true;
            return false;
        }

        private bool HasTheSamePropertyFeatures(Entry x, Entry y)
        {
            if (x.PropertyFeatures.Balconies.Equals(y.PropertyFeatures.Balconies) &&
                x.PropertyFeatures.OutdoorParkingPlaces.Equals(y.PropertyFeatures.OutdoorParkingPlaces) &&
                x.PropertyFeatures.IndoorParkingPlaces.Equals(y.PropertyFeatures.IndoorParkingPlaces) &&
                x.PropertyFeatures.BasementArea.Equals(y.PropertyFeatures.BasementArea))
                return true;
            return false;
        }

        private bool HasTheSamePropertyDetails(Entry x, Entry y)
        {
            if (x.PropertyDetails.Area.Equals(y.PropertyDetails.Area) &&
                x.PropertyDetails.FloorNumber.Equals(y.PropertyDetails.FloorNumber) &&
                x.PropertyDetails.NumberOfRooms.Equals(y.PropertyDetails.NumberOfRooms) &&
                x.PropertyDetails.YearOfConstruction.Equals(y.PropertyDetails.YearOfConstruction))
                return true;
            return false;
        }

        private bool HasTheSameOfferDetails(Entry x, Entry y)
        {
            if (x.OfferDetails.SellerContact.Name.Equals(y.OfferDetails.SellerContact.Name) &&
                x.OfferDetails.SellerContact.Telephone.Equals(y.OfferDetails.SellerContact.Telephone) &&
                x.OfferDetails.SellerContact.Email.Equals(y.OfferDetails.SellerContact.Email) &&
                x.OfferDetails.OfferKind.Equals(y.OfferDetails.OfferKind))
                return true;
            return false;
        }

        public bool Equals(Entry x, Entry y)
        {
            if (x.OfferDetails.Url.Equals(y.OfferDetails.Url))
                return true;
            // Not checking price because it can change from time to time
            // It can also be different on different pages due to commission
            if (HasTheSameAddress(x, y) && HasTheSamePropertyDetails(x, y) && 
                HasTheSamePropertyFeatures(x, y) && HasTheSameOfferDetails(x, y))
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            return obj.OfferDetails.Url == null ? 0 : obj.OfferDetails.Url.GetHashCode();
        }
    }
}
