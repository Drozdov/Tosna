using System;
using Tosna.Core.Common.Attributes;
using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class RandomizedBarometer : IBarometer
{
	private readonly Random random = new Random(239);

	public bool TryGetPressure(out float pressureHPa)
	{
		pressureHPa = 101f + (float)random.NextDouble() / 2;
		return true;
	}

	public BarometerDescription Description { get; } =
		new BarometerDescription(new GeographicalCoordinates(0, 0, 0), 1f);
}

[SerializableAs("RandomizedBarometer")]
public class RandomizedBarometerSignature : IDeviceSignature<RandomizedBarometer>
{
	public RandomizedBarometer CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new RandomizedBarometer();
	}
}