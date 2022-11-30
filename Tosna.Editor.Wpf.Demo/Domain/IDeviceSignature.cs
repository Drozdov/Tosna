using System.Collections.Generic;

namespace Tosna.Editor.Wpf.Demo.Domain;

public interface IDeviceSignature<out TDevice>
{
	TDevice CreateDevice(IDeviceSignatureResolver resolver);
}

public interface IDeviceSignatureResolver
{
	TDevice GetDevice<TDevice>(IDeviceSignature<TDevice> signature);
}

public class DeviceSignatureResolver : IDeviceSignatureResolver
{
	private readonly IDictionary<object, object> controllers = new Dictionary<object, object>();

	public TDevice GetDevice<TDevice>(IDeviceSignature<TDevice> signature)
	{
		if (controllers.TryGetValue(signature, out var value))
		{
			return (TDevice) value;
		}

		var result = signature.CreateDevice(this);
		controllers[signature] = result;
		return result;
	}
}