namespace SeaRoute.NET.Models;

public class FeatureCollection
{
    public string Type { get; set; } = "FeatureCollection";
    public List<Feature> Features { get; set; } = new();
}

public class Feature
{
    public string Type { get; set; } = "Feature";
    public Dictionary<string, object>? Properties { get; set; }
    public Geometry Geometry { get; set; } = null!;
}

public class Geometry
{
    public string Type { get; set; } = null!;
    public List<double[]> Coordinates { get; set; } = new();
}
