using System.IO;
using Tosna.Core.Common;
using Tosna.Core.Common.Stamps;
using Tosna.Core.IO;
using Tosna.Core.SerializationIterfaces;
using Tosna.Editor.Wpf.Demo.Domain.Common;
using Tosna.Editor.Wpf.Demo.Domain.Devices;

namespace Tosna.Editor.Wpf.Demo.Domain;

public static class EnvironmentFactory
{
	public static void SaveEnvironment(string initialFile)
	{
		var entryPointFilePath = new FileInfo(initialFile);
		var entryPointDirectory = entryPointFilePath.Directory;
		
		var device1 = new DummyDeviceSignature();
		var device2 = new DummyDeviceSignature();
		var device3 = new DummyDeviceSignature();

		var position1 = new GeographicalCoordinates(5, 10, 15);
		var position2 = new GeographicalCoordinates(31, 41, 9);
		var position3 = new GeographicalCoordinates(31, 53, 42);		
		
		var modBusAnemometer = new ModBusNetworkAnemometerSignature("192.168.32.13", 5566, position1);
		
		var weatherStation1 = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device1 },
			new IDeviceSignature<IBarometer>[] { device1 },
			new IDeviceSignature<IAnemometer>[] { device1, modBusAnemometer },
			position1,
			"Station1");

		var weatherStation2 = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device2 },
			new IDeviceSignature<IBarometer>[] { device2 },
			new IDeviceSignature<IAnemometer>[] { device2 },
			position2,
			"Station2");

		var weatherStation3 = new WeatherStationSignature(
			new IDeviceSignature<IThermometer>[] { device2, device3 },
			new IDeviceSignature<IBarometer>[] {  device2, device3 },
			new IDeviceSignature<IAnemometer>[] {  device2, device3 },
			position3,
			"Station3");

		var environment = new WeatherStationsEnvironmentSignature(
			new IDeviceSignature<WeatherStation>[] { weatherStation1, weatherStation2, weatherStation3 },
			"Environment1");

		var serializingElementsManager = new SerializingElementsManager();
		var signaturesSerializingTypeResolver = new SignaturesSerializingTypeResolver(serializingElementsManager);
		var constructor =
			new StampsCollectionConstructor(serializingElementsManager, signaturesSerializingTypeResolver);

		constructor.AddObject(environment);

		constructor.GetStamp(environment).UserId = "Environment1";

		FileInfo positionsFilePath;
		FileInfo devicesFilePath;

		if (entryPointDirectory != null)
		{
			positionsFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Details", "Positions.xml"));
			devicesFilePath = new FileInfo(Path.Combine(entryPointDirectory.FullName, "Details", "Devices.xml"));
		}
		else
		{
			positionsFilePath = new FileInfo(Path.Combine("Details", "Positions.xml"));
			devicesFilePath = new FileInfo(Path.Combine("Details", "Devices.xml"));
		}
		
		constructor.GetStamp(position1).UserId = "Position1";
		constructor.GetStamp(position1).UserFilePath = positionsFilePath;

		constructor.GetStamp(position2).UserId = "Position2";
		constructor.GetStamp(position2).UserFilePath = positionsFilePath;
		
		constructor.GetStamp(position3).UserId = "Position3";
		constructor.GetStamp(position3).UserFilePath = positionsFilePath;

		constructor.GetStamp(device1).UserId = "Device1";
		constructor.GetStamp(device1).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(device2).UserId = "Device2";
		constructor.GetStamp(device2).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(device3).UserId = "Device3";
		constructor.GetStamp(device3).UserFilePath = devicesFilePath;
		
		constructor.GetStamp(modBusAnemometer).UserId = "ModBusAnemometer";
		constructor.GetStamp(modBusAnemometer).UserFilePath = devicesFilePath;

		var stampsEnvironment = new StampsEnvironment(constructor.GetCollection(), entryPointFilePath);
		StampsEnvironmentWriter.Save(stampsEnvironment, new ImprintsSerializer(serializingElementsManager, signaturesSerializingTypeResolver));
	}
}