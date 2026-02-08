namespace SeaRoute.NET.Models;

public class GeoJsonPoint
{
    public string Type { get; set; } = "Feature";
    public Dictionary<string, object>? Properties { get; set; }
    public PointGeometry Geometry { get; set; } = null!;
}

public class PointGeometry
{
    public string Type { get; set; } = "Point";
    public double[] Coordinates { get; set; } = null!;
}
