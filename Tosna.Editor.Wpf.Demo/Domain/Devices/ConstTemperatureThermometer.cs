using Tosna.Editor.Wpf.Demo.Domain.Common;
using Tosna.Extensions;
using Tosna.Extensions.Serialization;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class ConstTemperatureThermometer : IThermometer
{
	public float TemperatureCelsius { get; }
	
	public ConstTemperatureThermometer(float temperatureCelsius, float temperatureAccuracy, GeographicalCoordinates position)
	{
		TemperatureCelsius = temperatureCelsius;
		Description = new ThermometerDescription(position, temperatureAccuracy);
	}

	public bool TryGetTemperature(out float temperatureCelsius)
	{
		temperatureCelsius = TemperatureCelsius;
		return true;
	}

	public ThermometerDescription Description { get; }
}

[SerializableAs("ConstTemperatureThermometer")]
public class ConstTemperatureThermometerSignature : IDeviceSignature<ConstTemperatureThermometer>
{
	public float TemperatureCelsius { get; }
	public float TemperatureAccuracy { get; }
	public GeographicalCoordinates Position { get; }

	public ConstTemperatureThermometerSignature(float temperatureCelsius, float temperatureAccuracy, GeographicalCoordinates position)
	{
		TemperatureCelsius = temperatureCelsius;
		TemperatureAccuracy = temperatureAccuracy;
		Position = position;
	}

	public ConstTemperatureThermometer CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new ConstTemperatureThermometer(TemperatureCelsius, TemperatureAccuracy, Position);
	}
}