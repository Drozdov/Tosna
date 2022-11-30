using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public interface IBarometer
{
	bool TryGetPressure(out float pressureHPa);
	
	BarometerDescription Description { get; }
}

public class BarometerDescription
{
	public GeographicalCoordinates Position { get; }
	
	public float PressureAccuracyHPa { get; }

	public BarometerDescription(GeographicalCoordinates position, float pressureAccuracyHPa)
	{
		Position = position;
		PressureAccuracyHPa = pressureAccuracyHPa;
	}
}