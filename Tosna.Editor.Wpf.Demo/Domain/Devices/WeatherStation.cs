using System.Linq;
using Tosna.Core.Common.Attributes;
using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class WeatherStation
{
	public IThermometer[] Thermometers { get; }
	
	public IBarometer[] Barometers { get; }
	
	public IAnemometer[] Anemometers { get; }
	
	public GeographicalCoordinates AveragePosition { get; }

	public string Name { get; }

	public WeatherStation(IThermometer[] thermometers, IBarometer[] barometers, IAnemometer[] anemometers, GeographicalCoordinates averagePosition, string name)
	{
		Thermometers = thermometers;
		Barometers = barometers;
		Anemometers = anemometers;
		AveragePosition = averagePosition;
		Name = name;
	}
}

[SerializableAs("WeatherStation")]
public class WeatherStationSignature : IDeviceSignature<WeatherStation>
{
	public IDeviceSignature<IThermometer>[] Thermometers { get; }
	
	public IDeviceSignature<IBarometer>[] Barometers { get; }
	
	public IDeviceSignature<IAnemometer>[] Anemometers { get; }
	
	public GeographicalCoordinates AveragePosition { get; }

	public string Name { get; }

	public WeatherStationSignature(IDeviceSignature<IThermometer>[] thermometers, IDeviceSignature<IBarometer>[] barometers, IDeviceSignature<IAnemometer>[] anemometers, GeographicalCoordinates averagePosition, string name)
	{
		Thermometers = thermometers;
		Barometers = barometers;
		Anemometers = anemometers;
		AveragePosition = averagePosition;
		Name = name;
	}

	public WeatherStation CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new WeatherStation(
			Thermometers.Select(resolver.GetDevice).ToArray(),
			Barometers.Select(resolver.GetDevice).ToArray(),
			Anemometers.Select(resolver.GetDevice).ToArray(),
			AveragePosition,
			Name
		);
	}
}