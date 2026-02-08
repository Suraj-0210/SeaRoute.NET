namespace SeaRoute.NET.Models;

public class GeoJsonLineString
{
    public string Type { get; set; } = "Feature";
    public LineStringProperties Properties { get; set; } = new();
    public LineStringGeometry Geometry { get; set; } = null!;
}

public class LineStringProperties
{
    public string Units { get; set; } = "nm";
    public double Length { get; set; }
}

public class LineStringGeometry
{
    public string Type { get; set; } = "LineString";
    public List<double[]> Coordinates { get; set; } = new();
}
