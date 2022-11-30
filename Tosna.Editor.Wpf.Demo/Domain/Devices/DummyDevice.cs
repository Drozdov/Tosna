using Tosna.Core.Common.Attributes;
using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class DummyDevice : IThermometer, IBarometer, IAnemometer
{
	bool IThermometer.TryGetTemperature(out float temperatureCelsius)
	{
		temperatureCelsius = default(float);
		return false;
	}

	bool IBarometer.TryGetPressure(out float pressureHPa)
	{
		pressureHPa = default(float);
		return false;
	}

	bool IAnemometer.TryGetWindSpeed(out float windSpeedMPerSecond, out float windAzimuthDegrees)
	{
		windSpeedMPerSecond = default(float);
		windAzimuthDegrees = default(float);
		return false;
	}

	AnemometerDescription IAnemometer.Description { get; } = new(new GeographicalCoordinates(0, 0, 0), 0, 0);

	BarometerDescription IBarometer.Description { get; } = new(new GeographicalCoordinates(0, 0, 0), 0);

	ThermometerDescription IThermometer.Description { get; } = new(new GeographicalCoordinates(0, 0, 0), 0);
}

[SerializableAs("DummyDevice")]
public class DummyDeviceSignature : IDeviceSignature<DummyDevice>
{
	public DummyDevice CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new DummyDevice();
	}
}