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

## 🚀 Endpoints

### POST `/api/flights/search`
```json
{
  "origin": "PAR",
  "maxPrice": 300,
  "sortBy": "price"
}
