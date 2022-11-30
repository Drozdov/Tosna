using Tosna.Core.Common.Attributes;
using Tosna.Editor.Wpf.Demo.Domain.Common;

namespace Tosna.Editor.Wpf.Demo.Domain.Devices;

public class ModBusNetworkAnemometer : IAnemometer
{
	private readonly string ipAddress;
	private readonly uint port;

	public ModBusNetworkAnemometer(string ipAddress, uint port, GeographicalCoordinates position)
	{
		this.ipAddress = ipAddress;
		this.port = port;

		Description = new AnemometerDescription(position, 0.1f, 0.1f);
	}

	public void Connect()
	{
		// Here goes all connection to remote address logics
	}

	public void Disconnect()
	{
		// Here goes all disconnection to remote address logics
	}

	public bool TryGetWindSpeed(out float windSpeedMPerSecond, out float windAzimuthDegrees)
	{
		// Here goes complicated logics of requesting actual values of wind speed

		windAzimuthDegrees = 0;
		windSpeedMPerSecond = 0;
		return true;
	}

	public AnemometerDescription Description { get; }
}

[SerializableAs("ModBusNetworkAnemometer")]
public class ModBusNetworkAnemometerSignature : IDeviceSignature<ModBusNetworkAnemometer>
{
	public string IpAddress { get; }
	
	public uint Port { get; }

	public GeographicalCoordinates Position { get; }
	
	public ModBusNetworkAnemometerSignature(string ipAddress, uint port, GeographicalCoordinates position)
	{
		IpAddress = ipAddress;
		Port = port;
		Position = position;
	}

	public ModBusNetworkAnemometer CreateDevice(IDeviceSignatureResolver resolver)
	{
		return new ModBusNetworkAnemometer(IpAddress, Port, Position);
	}
}