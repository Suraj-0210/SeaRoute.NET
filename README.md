# SeaRoute.NET

A .NET package for generating the shortest sea route between two points on Earth.

If points are on land, the function will attempt to find the nearest point on the sea and calculate the route from there.

**Not for routing purposes!** This library was developed to generate realistic-looking sea routes for visualizations of maritime routes, not for mariners to route their ships.

## Installation

```bash
dotnet add package SeaRoute.NET
```

Or via NuGet Package Manager:

```
Install-Package SeaRoute.NET
```

## Usage

### Basic Usage with GeoJSON Points

```csharp
using SeaRoute.NET;
using SeaRoute.NET.Models;

var calculator = new SeaRouteCalculator();

// Define origin and destination GeoJSON points
var origin = new GeoJsonPoint
{
    Type = "Feature",
    Properties = new Dictionary<string, object>(),
    Geometry = new PointGeometry
    {
        Type = "Point",
        Coordinates = new[] { 132.5390625, 21.616579336740603 }
    }
};

var destination = new GeoJsonPoint
{
    Type = "Feature",
    Properties = new Dictionary<string, object>(),
    Geometry = new PointGeometry
    {
        Type = "Point",
        Coordinates = new[] { -71.3671875, 75.05035357407698 }
    }
};

var route = calculator.CalculateRoute(origin, destination);
// Returns a GeoJSON LineString Feature

// Optionally, define the units for the length calculation
// Defaults to nautical miles ("nm"), can be "miles", "kilometers", or "km"
var routeMiles = calculator.CalculateRoute(origin, destination, "miles");

Console.WriteLine($"Route length: {route.Properties.Length} {route.Properties.Units}");
```

### Simplified Usage with Coordinates

```csharp
using SeaRoute.NET;

var calculator = new SeaRouteCalculator();

// Calculate route using longitude and latitude directly
var route = calculator.CalculateRoute(
    originLon: 132.5390625,
    originLat: 21.616579336740603,
    destLon: -71.3671875,
    destLat: 75.05035357407698,
    units: "nm"
);

if (route != null)
{
    Console.WriteLine($"Route found with {route.Geometry.Coordinates.Count} waypoints");
    Console.WriteLine($"Total distance: {route.Properties.Length} {route.Properties.Units}");
}
else
{
    Console.WriteLine("No route found");
}
```

### Working with the Result

```csharp
var route = calculator.CalculateRoute(originLon, originLat, destLon, destLat);

if (route != null)
{
    // Access route properties
    var length = route.Properties.Length;
    var units = route.Properties.Units;
    
    // Access route coordinates
    foreach (var coordinate in route.Geometry.Coordinates)
    {
        var lon = coordinate[0];
        var lat = coordinate[1];
        Console.WriteLine($"Waypoint: {lon}, {lat}");
    }
    
    // Serialize to JSON
    var json = Newtonsoft.Json.JsonConvert.SerializeObject(route, Newtonsoft.Json.Formatting.Indented);
    Console.WriteLine(json);
}
```

## Supported Units

- `"nm"` - Nautical miles (default)
- `"miles"` - Statute miles
- `"kilometers"` or `"km"` - Kilometers

## Features

- Calculate shortest sea routes using Dijkstra's algorithm
- Automatic snapping of land points to nearest sea network vertex
- **Dynamically add ports to the maritime network** for better coverage
- Support for multiple distance units
- Returns GeoJSON-compliant LineString features
- Embedded maritime network data (no external files needed)
- Built-in port registry for common worldwide ports

## Adding Custom Ports

If a port is not well-covered by the default maritime network, you can add it dynamically:

```csharp
var calculator = new SeaRouteCalculator();

// Add a single port
calculator.AddPortToNetwork(
    portLon: 1.3515,
    portLat: 51.9542,
    portName: "Felixstowe",
    maxConnectionDistanceKm: 200
);

// Add multiple ports using the built-in registry
calculator.AddPortsToNetwork(PortRegistry.UKPorts.GetAll());
calculator.AddPortsToNetwork(PortRegistry.EuropeanPorts.GetAll());

// Now calculate routes to/from these ports
var route = calculator.CalculateRoute(-46.3017, -23.954, 1.3515, 51.9542);
```

See [ADDING_PORTS.md](ADDING_PORTS.md) for detailed instructions and best practices.

## Technical Details

The package uses:
- **Dijkstra's algorithm** for pathfinding on the maritime network
- **Haversine formula** for distance calculations
- **Rhumb line distance** for point-to-network snapping
- Maritime network data derived from Eurostat's marnet dataset

## Credits

This is a .NET port of the [searoute-js](https://github.com/johnx25bd/searoute) npm package.

Based on Eurostat's [Searoute Java library](https://github.com/eurostat/searoute) (EUPL-1.2 licensed).

The maritime network data is derived from Eurostat's marnet dataset, which incorporates:
- [Oak Ridge National Labs CTA Transportation Network Group](https://cta.ornl.gov/transnet/), Global Shipping Lane Network (2000)
- Additional European coastal routes based on AIS data (Eurostat)

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](https://mozilla.org/MPL/2.0/).

## Author

.NET port created for the SeaRoute.NET project.
