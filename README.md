# ✈️ DealEngine Flight Search API

Esta es una solución al reto técnico de DealEngine para construir una API REST que integre con la API de Amadeus y permita consultar precios de vuelos.

## 🚀 Descripción

El proyecto implementa una API que permite buscar vuelos mediante la API Self-Service de Amadeus. Los resultados son devueltos en formato JSON, siguiendo una estructura consistente, e incluyen datos como:

- Código IATA de origen y destino.
- Aerolínea.
- Número de vuelo.
- Precio y moneda.

## 📦 Características

- API REST en .NET 8.
- Autenticación con Amadeus vía OAuth2.
- Búsqueda de vuelos y destinos.
- Mapeo de resultados personalizados.
- Manejo de errores.
- Inyección de dependencias.
- Buenas prácticas con interfaces y servicios.
- Pruebas unitarias básicas con NUnit.
- Configuración por variables de entorno.
- Patrón Retry ante errores HTTP transitorios.

## 🛠️ Tecnologías usadas

- ASP.NET Core 8
- C#
- HttpClientFactory
- Automapper
- Polly (retry policy)
- NUnit + Moq (pruebas unitarias)

---

## 🔧 Configuración

### 1. Clona el repositorio

```bash
git clone https://github.com/Themendez/dealEngine-TechChallenge.git
cd dealEngine-TechChallenge/dealEngine.backend

### **2. Configura tus variables de entorno**
Crea un archivo appsettings.Development.json o configura variables de entorno directamente.

{
  "Amadeus": {
    "ClientId": "TU_CLIENT_ID",
    "ClientSecret": "TU_CLIENT_SECRET",
    "BaseUrl": "https://test.api.amadeus.com"
  }
}

### **3. Ejecuta el proyecto**
  dotnet run

## 🧪 Endpoints disponibles

### `POST /api/flights/search`

Busca destinos desde un origen según las preferencias del usuario.

#### 📥 Request Body
```json
{
  "origin": "MEX",
  "oneWay": false,
  "nonStop": false,
  "maxPrice": 200,
  "viewBy": "DATE"
}
####📤 Response

{
  "success": true,
  "data": [
    {
      "origin": "MEX",
      "destination": "LAX",
      "price": "200.00",
      "currency": "USD"
    }
  ]
}

POST /api/flights/offers
Busca ofertas completas de vuelo: itinerarios, aerolínea, número de vuelo y precio.

####📥 Request Body
  {
    "currencyCode": "USD",
    "originDestinations": [
      {
        "id": "1",
        "originLocationCode": "NYC",
        "destinationLocationCode": "MAD",
        "departureDateTimeRange": {
          "date": "2025-11-01",
          "time": "10:00:00"
        }
      }
    ],
    "travelers": [
      {
        "id": "1",
        "travelerType": "ADULT"
      }
    ],
    "sources": ["GDS"],
    "searchCriteria": {
      "maxFlightOffers": 2,
      "flightFilters": {
        "cabinRestrictions": [
          {
            "cabin": "BUSINESS",
            "coverage": "MOST_SEGMENTS",
            "originDestinationIds": ["1"]
          }
        ]
      }
    }
  }
#### 📤 Response
{
  "success": true,
  "data": [
    {
      "origin": "NYC",
      "destination": "MAD",
      "airline": "IB",
      "flightNumber": "6254",
      "price": "950.00",
      "currency": "USD"
    }
  ]
}

## ✅ Pruebas
Se incluye un conjunto básico de pruebas unitarias usando NUnit y Moq.

### Ejecutar pruebas
dotnet test

### Las pruebas cubren:

- Autenticación y manejo de token.
- Lógica de búsqueda de vuelos.
- Respuestas válidas del servicio simulado.
- Casos de error y validaciones.

## 📄 Consideraciones
Se usa Polly para reintentos automáticos en errores 429 o problemas de red.
Las credenciales sensibles se configuran por medio de variables de entorno o appsettings.json (no deben incluirse en el repositorio).
Código modular, mantenible y listo para escalamiento.
