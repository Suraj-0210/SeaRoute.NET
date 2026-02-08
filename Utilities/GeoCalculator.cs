namespace SeaRoute.NET.Utilities;

public static class GeoCalculator
{
    private const double EarthRadiusKm = 6371.0;
    private const double EarthRadiusMiles = 3958.8;
    private const double EarthRadiusNauticalMiles = 3440.065;

    public static double RhumbDistance(double[] point1, double[] point2, string units = "km")
    {
        double lat1 = point1[1] * Math.PI / 180;
        double lat2 = point2[1] * Math.PI / 180;
        double dLat = lat2 - lat1;
        double dLon = Math.Abs(point2[0] - point1[0]) * Math.PI / 180;

        double dPhi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
        double q = Math.Abs(dPhi) > 10e-12 ? dLat / dPhi : Math.Cos(lat1);

        if (Math.Abs(dLon) > Math.PI)
        {
            dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
        }

        double dist = Math.Sqrt(dLat * dLat + q * q * dLon * dLon);
        
        return units switch
        {
            "miles" => dist * EarthRadiusMiles,
            "nm" => dist * EarthRadiusNauticalMiles,
            "kilometers" or "km" => dist * EarthRadiusKm,
            _ => dist * EarthRadiusKm
        };
    }

    public static double PointToLineDistance(double[] point, List<double[]> lineCoords, string units = "km")
    {
        double minDistance = double.MaxValue;

        for (int i = 0; i < lineCoords.Count - 1; i++)
        {
            double dist = PointToSegmentDistance(point, lineCoords[i], lineCoords[i + 1]);
            if (dist < minDistance)
                minDistance = dist;
        }

        return ConvertDistance(minDistance, "km", units);
    }

    private static double PointToSegmentDistance(double[] point, double[] segStart, double[] segEnd)
    {
        double x = point[0];
        double y = point[1];
        double x1 = segStart[0];
        double y1 = segStart[1];
        double x2 = segEnd[0];
        double y2 = segEnd[1];

        double A = x - x1;
        double B = y - y1;
        double C = x2 - x1;
        double D = y2 - y1;

        double dot = A * C + B * D;
        double lenSq = C * C + D * D;
        double param = lenSq != 0 ? dot / lenSq : -1;

        double xx, yy;

        if (param < 0)
        {
            xx = x1;
            yy = y1;
        }
        else if (param > 1)
        {
            xx = x2;
            yy = y2;
        }
        else
        {
            xx = x1 + param * C;
            yy = y1 + param * D;
        }

        return HaversineDistance(new[] { x, y }, new[] { xx, yy });
    }

    public static double HaversineDistance(double[] coord1, double[] coord2)
    {
        double lat1 = coord1[1] * Math.PI / 180;
        double lat2 = coord2[1] * Math.PI / 180;
        double dLat = (coord2[1] - coord1[1]) * Math.PI / 180;
        double dLon = (coord2[0] - coord1[0]) * Math.PI / 180;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    public static double CalculateLineStringLength(List<double[]> coordinates, string units = "nm")
    {
        double totalLength = 0;

        for (int i = 0; i < coordinates.Count - 1; i++)
        {
            totalLength += HaversineDistance(coordinates[i], coordinates[i + 1]);
        }

        return ConvertDistance(totalLength, "km", units);
    }

    private static double ConvertDistance(double distance, string fromUnits, string toUnits)
    {
        double distanceInKm = fromUnits switch
        {
            "miles" => distance / EarthRadiusMiles * EarthRadiusKm,
            "nm" => distance / EarthRadiusNauticalMiles * EarthRadiusKm,
            _ => distance
        };

        return toUnits switch
        {
            "miles" => distanceInKm / EarthRadiusKm * EarthRadiusMiles,
            "nm" => distanceInKm / EarthRadiusKm * EarthRadiusNauticalMiles,
            _ => distanceInKm
        };
    }
}
