using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Common.Attributes;
using Tosna.Editor.Wpf.Demo.Domain.Devices;

namespace Tosna.Editor.Wpf.Demo.Domain;

public class WeatherStationsEnvironment
{
	public IReadOnlyCollection<WeatherStation> WeatherStations { get; }
	
	public string Name { get; }

	public WeatherStationsEnvironment(IReadOnlyCollection<WeatherStation> weatherStations, string name)
	{
		WeatherStations = weatherStations;
		Name = name;
	}
}

[SerializableAs("WeatherStationsEnvironment")]
public class WeatherStationsEnvironmentSignature : IDeviceSignature<WeatherStationsEnvironment>
{
	public IDeviceSignature<WeatherStation>[] WeatherStations { get; }
	
	public string Name { get; }

	public WeatherStationsEnvironmentSignature(IDeviceSignature<WeatherStation>[] weatherStations, string name)
	{
		WeatherStations = weatherStations;
		Name = name;
	}

	public WeatherStationsEnvironment CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new WeatherStationsEnvironment(WeatherStations.Select(resolver.GetDevice).ToArray(), Name);
	}
}