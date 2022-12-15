using System;
using Tosna.Editor.Wpf.Demo.Domain.Common;
using Tosna.Extensions.Serialization;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class RandomizedThermometer : IThermometer
{
	private readonly float[] availableTemperatures;

	private readonly Random random = new Random();

	public RandomizedThermometer(float[] availableTemperatures)
	{
		this.availableTemperatures = availableTemperatures;

		Description = new ThermometerDescription(new GeographicalCoordinates(0, 0, 0), 1);
	}

	public bool TryGetTemperature(out float temperatureCelsius)
	{
		if (availableTemperatures.Length == 0)
		{
			temperatureCelsius = default(float);
			return false;
		}
		
		temperatureCelsius = availableTemperatures[random.Next(availableTemperatures.Length)];
		return true;
	}

	public ThermometerDescription Description { get; }
}

[SerializableAs("RandomizedThermometer")]
public class RandomizedThermometerSignature : IDeviceSignature<RandomizedThermometer>
{
	public float[] AvailableTemperatures { get; }

	public RandomizedThermometerSignature(float[] availableTemperatures)
	{
		AvailableTemperatures = availableTemperatures;
	}
	
	public RandomizedThermometer CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new RandomizedThermometer(AvailableTemperatures);
	}
}