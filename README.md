# dealEngine-TechChallenge
this is a technical challenge for deal engine company
This Web API provides endpoints to search for flights using Amadeus' Self-Service API.

## 🔧 Features
- Token caching
- Flight search sorted by user preference
- RESTful endpoints with consistent JSON
- Error handling
- Easily testable via dependency injection

## 🛠 Technologies
- .NET 8
- C#
- HttpClient
- Dependency Injection
- Amadeus Self-Service API
- AutoMapper

## 🚀 Endpoints

### GET `/api/flights/token`
No parameters

### POST `/api/flights/search`
```json
origin is requiered
{
  "origin": "string", 
  "departureDate": "string",
  "oneWay": false,
  "nonStop": false,
  "maxPrice": 0,
  "viewBy": 0
}

###POST /api/Flights/flight-offers

{
  "currencyCode": "string",
  "originDestinations": [
    {
      "id": "string",
      "originLocationCode": "string",
      "destinationLocationCode": "string",
      "departureDateTimeRange": {
        "date": "string",
        "time": "string"
      }
    }
  ],
  "travelers": [
    {
      "id": "string",
      "travelerType": "string"
    }
  ],
  "sources": [
    "string"
  ],
  "searchCriteria": {
    "maxFlightOffers": 0,
    "flightFilters": {
      "cabinRestrictions": [
        {
          "cabin": "string",
          "coverage": "string",
          "originDestinationIds": [
            "string"
          ]
        }
      ]
    }
  }
}


### /api/Flights/locations
{
  "pageNumber": 0,
  "pageSize": 0,
  "keyword": "string",
  "subType": "string",
  "countryCode": "string",
  "sort": "string",
  "view": "string"
}

