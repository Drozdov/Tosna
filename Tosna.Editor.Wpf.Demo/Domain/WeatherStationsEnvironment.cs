using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Common.Attributes;
using Tosna.Editor.IDE.FieldsConfigurator;
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

	public void Init()
	{
		foreach (var weatherStation in WeatherStations)
		{
			var connectableDevices = new IConnectable[] { }
				.Union(weatherStation.Anemometers.OfType<IConnectable>())
				.Union(weatherStation.Barometers.OfType<IConnectable>())
				.Union(weatherStation.Thermometers.OfType<IConnectable>());
			foreach (var connectable in connectableDevices)
			{
				connectable.Connect();
			}
		}
	}
}

[SerializableAs("WeatherStationsEnvironment")]
[FieldsConfigurator(typeof(WeatherStationsEnvironmentContext))]
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

public class WeatherStationsEnvironmentContext : FieldsConfiguratorContext
{
	public WeatherStationsEnvironmentContext() : base(new Dictionary<string, string>
	{
		{nameof(WeatherStationsEnvironmentSignature.Name), "Name"},
		{nameof(WeatherStationsEnvironmentSignature.WeatherStations), "Weather stations"}
	})
	{
	}
}