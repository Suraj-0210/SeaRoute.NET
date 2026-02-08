using SeaRoute.NET.Models;

namespace SeaRoute.NET.Algorithms;

public class DijkstraPathFinder
{
    private readonly FeatureCollection _network;
    private readonly Dictionary<string, List<Edge>> _graph;
    private readonly Dictionary<string, double[]> _vertices;

    public DijkstraPathFinder(FeatureCollection network)
    {
        _network = network;
        _graph = new Dictionary<string, List<Edge>>();
        _vertices = new Dictionary<string, double[]>();
        BuildGraph();
    }

    private void BuildGraph()
    {
        foreach (var feature in _network.Features)
        {
            if (feature.Geometry.Type != "LineString") continue;

            var coords = feature.Geometry.Coordinates;
            for (int i = 0; i < coords.Count - 1; i++)
            {
                var from = CoordToKey(coords[i]);
                var to = CoordToKey(coords[i + 1]);

                _vertices[from] = coords[i];
                _vertices[to] = coords[i + 1];

                double distance = CalculateDistance(coords[i], coords[i + 1]);

                if (!_graph.ContainsKey(from))
                    _graph[from] = new List<Edge>();
                if (!_graph.ContainsKey(to))
                    _graph[to] = new List<Edge>();

                _graph[from].Add(new Edge(to, distance));
                _graph[to].Add(new Edge(from, distance));
            }
        }
    }

    public PathResult? FindPath(double[] start, double[] end)
    {
        string startKey = CoordToKey(start);
        string endKey = CoordToKey(end);

        if (!_vertices.ContainsKey(startKey) || !_vertices.ContainsKey(endKey))
            return null;

        var distances = new Dictionary<string, double>();
        var previous = new Dictionary<string, string?>();
        var priorityQueue = new PriorityQueue<string, double>();

        distances[startKey] = 0;
        priorityQueue.Enqueue(startKey, 0);

        while (priorityQueue.Count > 0)
        {
            string current = priorityQueue.Dequeue();

            if (current == endKey)
                break;

            if (!distances.TryGetValue(current, out double currentDistance))
                continue;

            if (_graph.TryGetValue(current, out var edges))
            {
                foreach (var edge in edges)
                {
                    double alt = currentDistance + edge.Weight;
                    
                    if (!distances.TryGetValue(edge.To, out double existingDistance) || alt < existingDistance)
                    {
                        distances[edge.To] = alt;
                        previous[edge.To] = current;
                        priorityQueue.Enqueue(edge.To, alt);
                    }
                }
            }
        }

        if (!previous.ContainsKey(endKey) && startKey != endKey)
            return null;

        var path = new List<double[]>();
        string? currentNode = endKey;

        while (currentNode != null)
        {
            path.Add(_vertices[currentNode]);
            currentNode = previous.GetValueOrDefault(currentNode);
        }

        path.Reverse();

        return new PathResult
        {
            Path = path,
            Distance = distances.GetValueOrDefault(endKey, double.MaxValue)
        };
    }

    private static string CoordToKey(double[] coord)
    {
        return $"{coord[0]:F6},{coord[1]:F6}";
    }

    private static double CalculateDistance(double[] coord1, double[] coord2)
    {
        const double R = 6371; // Earth radius in km
        double lat1 = coord1[1] * Math.PI / 180;
        double lat2 = coord2[1] * Math.PI / 180;
        double dLat = (coord2[1] - coord1[1]) * Math.PI / 180;
        double dLon = (coord2[0] - coord1[0]) * Math.PI / 180;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private class Edge
    {
        public string To { get; }
        public double Weight { get; }

        public Edge(string to, double weight)
        {
            To = to;
            Weight = weight;
        }
    }
}

public class PathResult
{
    public List<double[]> Path { get; set; } = new();
    public double Distance { get; set; }
}
