using System.Reflection;
using Newtonsoft.Json;
using SeaRoute.NET.Algorithms;
using SeaRoute.NET.Models;
using SeaRoute.NET.Utilities;

namespace SeaRoute.NET;

public class SeaRouteCalculator
{
    private readonly DijkstraPathFinder _pathFinder;
    private readonly FeatureCollection _marnet;

    public SeaRouteCalculator()
    {
        _marnet = LoadMarnetData();
        _pathFinder = new DijkstraPathFinder(_marnet);
    }

    /// <summary>
    /// Calculate the shortest sea route between two points
    /// </summary>
    /// <param name="origin">Origin point as GeoJSON Feature</param>
    /// <param name="destination">Destination point as GeoJSON Feature</param>
    /// <param name="units">Units for length calculation: "nm" (nautical miles), "miles", "kilometers", "km". Default is "nm"</param>
    /// <returns>GeoJSON LineString Feature representing the route, or null if no route found</returns>
    public GeoJsonLineString? CalculateRoute(GeoJsonPoint origin, GeoJsonPoint destination, string units = "nm")
    {
        try
        {
            var snappedOrigin = SnapToNetwork(origin.Geometry.Coordinates);
            var snappedDestination = SnapToNetwork(destination.Geometry.Coordinates);

            var route = _pathFinder.FindPath(snappedOrigin, snappedDestination);

            if (route == null)
            {
                Console.WriteLine("No route found");
                return null;
            }

            double length = GeoCalculator.CalculateLineStringLength(route.Path, units);

            return new GeoJsonLineString
            {
                Type = "Feature",
                Properties = new LineStringProperties
                {
                    Units = units,
                    Length = length
                },
                Geometry = new LineStringGeometry
                {
                    Type = "LineString",
                    Coordinates = route.Path
                }
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calculating route: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Calculate the shortest sea route between two coordinate points
    /// </summary>
    /// <param name="originLon">Origin longitude</param>
    /// <param name="originLat">Origin latitude</param>
    /// <param name="destLon">Destination longitude</param>
    /// <param name="destLat">Destination latitude</param>
    /// <param name="units">Units for length calculation: "nm" (nautical miles), "miles", "kilometers", "km". Default is "nm"</param>
    /// <returns>GeoJSON LineString Feature representing the route, or null if no route found</returns>
    public GeoJsonLineString? CalculateRoute(double originLon, double originLat, double destLon, double destLat, string units = "nm")
    {
        var origin = new GeoJsonPoint
        {
            Geometry = new PointGeometry
            {
                Coordinates = new[] { originLon, originLat }
            }
        };

        var destination = new GeoJsonPoint
        {
            Geometry = new PointGeometry
            {
                Coordinates = new[] { destLon, destLat }
            }
        };

        return CalculateRoute(origin, destination, units);
    }

    private double[] SnapToNetwork(double[] point)
    {
        int nearestLineIndex = 0;
        double minDistance = 30000;

        for (int i = 0; i < _marnet.Features.Count; i++)
        {
            var feature = _marnet.Features[i];
            double dist = GeoCalculator.PointToLineDistance(point, feature.Geometry.Coordinates, "kilometers");
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestLineIndex = i;
            }
        }

        double? nearestVertexDist = null;
        double[]? nearestCoord = null;

        foreach (var coord in _marnet.Features[nearestLineIndex].Geometry.Coordinates)
        {
            double distToVertex = GeoCalculator.RhumbDistance(point, coord, "km");

            if (nearestVertexDist == null || distToVertex < nearestVertexDist)
            {
                nearestVertexDist = distToVertex;
                nearestCoord = coord;
            }
        }

        return nearestCoord ?? point;
    }

    private static FeatureCollection LoadMarnetData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SeaRoute.NET.Data.marnet_densified.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        
        return JsonConvert.DeserializeObject<FeatureCollection>(json) 
               ?? throw new InvalidOperationException("Failed to deserialize marnet data");
    }
}
