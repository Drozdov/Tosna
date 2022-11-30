using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public interface IAnemometer
{
	bool TryGetWindSpeed(out float windSpeedMPerSecond, out float windAzimuthDegrees);
	
	AnemometerDescription Description { get; }
}

public class AnemometerDescription
{
	public GeographicalCoordinates Position { get; }
	
	public float WindSpeedAccuracyMPerSecond { get; }
	
	public float WindAzimuthAccuracyDegrees { get; }

	public AnemometerDescription(GeographicalCoordinates position, float windSpeedAccuracyMPerSecond, float windAzimuthAccuracyDegrees)
	{
		Position = position;
		WindSpeedAccuracyMPerSecond = windSpeedAccuracyMPerSecond;
		WindAzimuthAccuracyDegrees = windAzimuthAccuracyDegrees;
	}
}