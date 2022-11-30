using Tosna.Core.Common.Attributes;

namespace Tosna.Editor.Wpf.Demo.Domain.Common;

[SerializableAs("GeographicalCoordinates")]
public class GeographicalCoordinates
{
	public double Latitude { get; }
	
	public double Longitude { get; }
	
	public double Altitude { get; }

	public GeographicalCoordinates(double latitude, double longitude, double altitude)
	{
		Latitude = latitude;
		Longitude = longitude;
		Altitude = altitude;
	}
}