
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderDomain.Orders
{
   public class Address: ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string Country { get; }
        public string ZipCode { get; } 
        private Address()
        {

        }

        public Address(string street, string city, string state, string country, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return City;
            yield return State;
            yield return Street;
            yield return ZipCode;
            yield return Country;
        }
    }
}
