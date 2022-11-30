using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public interface IThermometer
{
	bool TryGetTemperature(out float temperatureCelsius);
	
	ThermometerDescription Description { get; }
}

public class ThermometerDescription
{
	public GeographicalCoordinates Position { get; }
	
	public float TemperatureAccuracyCelsius { get; }

	public ThermometerDescription(GeographicalCoordinates position, float temperatureAccuracyCelsius)
	{
		Position = position;
		TemperatureAccuracyCelsius = temperatureAccuracyCelsius;
	}
}