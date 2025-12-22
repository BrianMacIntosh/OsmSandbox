using System.Collections.Generic;
using System.Device.Location;

public class FeatureCollection
{
	public string type;
	public string generator;
	public string copyright;
	public string timestamp;
	public List<Feature> features = new List<Feature>();
}

public class Feature
{
	public string type;
	public Dictionary<string, string> properties;
	public Geometry geometry;
	public string id;
}

public class Geometry
{
	public string type;
	public double[][][] coordinates;

	public GeoCoordinate GetCoordinate(int setIndex, int node)
	{
		return new GeoCoordinate(coordinates[setIndex][node][1], coordinates[setIndex][node][0]);
	}

	public GeoCoordinate[] GetCoordinates(int setIndex)
	{
		double[][] set = coordinates[setIndex];
		int nodeCount = set.Length;
		GeoCoordinate[] coords = new GeoCoordinate[nodeCount];
		for (int index = 0; index < nodeCount; ++index)
		{
			coords[index] = new GeoCoordinate(set[index][1], set[index][0]);
		}
		return coords;
	}
}
